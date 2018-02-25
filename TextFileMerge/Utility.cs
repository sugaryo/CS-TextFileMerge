using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextFileMerge
{
	class Utility
	{
		
		public enum FolderOption
		{
			/// <summary>
			/// パスにフォルダが含まれる場合、これを無視します。
			/// </summary>
			Ignore,

			/// <summary>
			/// パスにフォルダが含まれる場合、直下のファイルのみ列挙します。（ShallowSearch）
			/// </summary>
			/// <seealso cref="SearchOption.TopDirectoryOnly"/>
			SearchFilesShallow,

			/// <summary>
			/// パスにフォルダが含まれる場合、配下のファイルを全て列挙します。（DeepSearch）
			/// </summary>
			/// <seealso cref="SearchOption.AllDirectories"/>
			SearchFilesDeep,
		}

		public static IEnumerable<FileInfo> EachFile( 
				IEnumerable<string> paths, 
				FolderOption folder = FolderOption.Ignore )
		{
			foreach ( var path in paths )
			{
				// フォルダが存在する場合
				if ( Directory.Exists( path ) )
				{
					switch ( folder )
					{
						case FolderOption.SearchFilesShallow:
							foreach ( var file in Directory.EnumerateFiles( 
									path, "*.*", 
									SearchOption.TopDirectoryOnly ) )
							{
								yield return new FileInfo( file );
							}
							break;

						case FolderOption.SearchFilesDeep:
							foreach ( var file in Directory.EnumerateFiles( 
									path, "*.*", 
									SearchOption.AllDirectories ) )
							{
								yield return new FileInfo( file );
							}
							break;

						case FolderOption.Ignore:
						default:
							continue;
					}
				}

				// ファイルが存在する場合
				if ( File.Exists( path ) )
				{
					yield return new FileInfo( path );
				}
			}
		}
		
	}
}
