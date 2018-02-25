using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Compression;

using ArgsAnalyzer;


namespace TextFileMerge
{
	class Program
	{
		static void Main( string[] args )
		{
			try
			{
				Execute( args );
			}
			catch ( Exception ex )
			{
				Console.WriteLine( ex.Message );
				Console.WriteLine( ex.StackTrace );
			}
		}

		private static void Execute( string[] args )
		{
			var arguments = args.parse();
			
			// autoオプションを取得。
			var auto = arguments
				.AsValueOptions()
				.Where ( x => x.Name == "auto" )
				.FirstOrDefault();
			
			if ( null != auto )
			{
				// auto-compress mode では追加のオプションをチェック。
				var s = arguments
					.AsPropertyOptions()
					.Where( x => x.Name == "size"
							  || x.Name == "s" )
					.FirstOrDefault();
				int size = int.Parse( s?.value ?? "10" );
				
				var del = arguments
					.AsValueOptions()
					.Where( x => x.Name == "delete"
							  || x.Name == "del"
							  || x.Name == "d" )
					.FirstOrDefault();
				bool delete = null != del;
				
				AutoCompress( size, delete );
				return;
			}


			// パラメータからファイルパスを引っこ抜く。
			var files = arguments
				.AsParameters()
				.Select( x => new FileInfo(x) )
				.Where( file => file.Exists )
				.ToList();

			// archiveオプションを取得。
			var a = arguments
				.AsPropertyOptions()
				.Where( x => x.Name == "a"
						  || x.Name == "acv"
						  || x.Name == "arc"
						  || x.Name == "archive" )
				.FirstOrDefault();

			
			if ( 0 == files.Count )
			{
				Console.WriteLine( "ファイルが１個もドロップされてねーずら。" );
				return;
			}


			if ( null != a )
			{
				// アーカイブファイル名が指定された場合はそのまま使う。
				string name = a.value;

				Compress( files, name );
			}
			else
			{
				// アーカイブファイル名がオプションで指定されなかった場合は
				// コンソール入力からアーカイブファイル名を指定する。
				Console.Write( "input archive-filename:" );
				string name = Console.ReadLine();

				if ( name == "exit" ) return;

				Compress( files, name );
			}
		}


		private class GroupBy<TKey, TElement> 
		{
			private readonly Dictionary<TKey, List<TElement>> map = new Dictionary<TKey, List<TElement>>();

			public void Add( TKey key, params TElement[] elements )
			{
				if ( !this.map.ContainsKey( key ) )
				{
					this.map.Add( key, new List<TElement>() );
				}

				this.map[key].AddRange( elements );
			}

			public IEnumerable<TKey> Keys
			{
				get {
					return this.map.Keys;
				}
			}

			public List<TElement> this[TKey key]
			{
				get {
					return this.map[key];
				}
			}
		}


		private static void AutoCompress( int size, bool delete )
		{
			string current = Environment.CurrentDirectory;
			string[] paths = {
				current
			};

			Console.WriteLine( "execute AutoCompressMode;" );
			Console.WriteLine( $" -   size: {size}"  );
			Console.WriteLine( $" - delete: {delete}" );
			Console.WriteLine( $" -   path: {current}" );
			Console.WriteLine();


#warning EndsWithは後でsurfixオプションに変更するとして、取り敢えず今はべた書き。
			var infos = Utility
				.EachFile( paths, Utility.FolderOption.SearchFilesShallow )
				.Where( x => x.Name.EndsWith( ".url.txt" ) );
			

			GroupBy<string, FileInfo> group = new  GroupBy<string, FileInfo>();
			foreach ( FileInfo file in infos )
			{
				string name = file.Name;

				string key = name.split(".")[0];

				group.Add( key, file );
			}

			
			var keys = group.Keys;
			foreach ( var key in keys )
			{
				var files = group[key];

				if ( size < files.Count )
				{
					Compress( files, key );
					Zip( files, key, delete );
				}
			}
		}
		
		private static void Zip( List<FileInfo> files, string name, bool delete )
		{
			string timestamp = DateTime.Now.ToString("yyyy-MMdd-HHmmss");
			string path = $"{name}.{timestamp}.zip";


			// ZIPに固める
			using ( var zip = ZipFile.Open( path, ZipArchiveMode.Create ) )
			{
				foreach ( var file in files )
				{
					zip.CreateEntryFromFile( file.FullName, file.Name );
				}
			}

			// マージ、ZIPが済んだファイルは削除する。
			Console.WriteLine( $"auto-compressed [key:{name}]" );
			foreach ( var file in files )
			{
				Console.WriteLine( "  - " + file.Name );
			}
			if ( delete )
			{
				Console.WriteLine( "  >> delete files;" );
				foreach ( var file in files )
				{
					File.Delete( file.FullName );
				}
				Console.WriteLine( "  >> deleted." );
			}
		}


		#region Compress
		private static void Compress( IEnumerable<FileInfo> files, string name )
		{
#warning nameはPath::InvalidPathCharsエスケープするべきだけど、取り敢えず使うのは自分だからスルーしとく。

			string timestamp = DateTime.Now.ToString("yyyy-MMdd-HHmmss");
			string path = $"{name}.archived.{timestamp}.txt";
			
		
			// 取り敢えず最低限、重複排除するだけで実装。
			HashSet<string> unique = new HashSet<string>();

			using ( StreamWriter writer = File.CreateText( path ) )
			{
				foreach ( var file in files )
				{
					var lines = File.ReadAllLines(file.FullName);
					foreach ( var line in lines )
					{
						// 初出なら作ったファイルに書き込み。
						if ( unique.Add( line ) )
						{
							writer.WriteLine( line );
						}
					}
				}
			}
		}
		#endregion
	}
}
