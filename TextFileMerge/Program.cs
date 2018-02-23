using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
			var files = args
				.AsEnumerable()
				.Select( x => new FileInfo(x) )
				.Where( file => file.Exists )
				.ToList();

			if ( 0 == files.Count )
			{
				Console.WriteLine( "ファイルが１個もドロップされてねーずら。" );
				return;
			}

			// アーカイブファイル名をコンソールから入力する事にした。
			Console.Write( "input archive-filename:" );
			string name = Console.ReadLine();

			if ( name == "exit" ) return;
			
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
	}
}
