using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


			// アーカイブファイル名がオプションで指定されなかった場合は
			// コンソール入力からアーカイブファイル名を指定する。
			if ( null == a )
			{
				Console.Write( "input archive-filename:" );
				string name = Console.ReadLine();

				if ( name == "exit" ) return;
				
				Compress( files, name );
			}
			// アーカイブファイル名が指定された場合はそのまま使う。
			else
			{
				string name = a.value;

				Compress( files, name );
			}
		}

		#region Compress
		private static void Compress( List<FileInfo> files, string name )
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
