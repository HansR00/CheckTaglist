using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CheckTaglist
{
    internal class CheckTaglist
    {

        static async Task Main()
        {
            try
            {
                string[] ExistingTags = File.ReadAllLines( "./WebTags.txt" );
                var wikiUrl = "https://cumuluswiki.org/a/Full_list_of_Webtags";
                string WikiTaglist; // = File.ReadAllText( "./WikiWebtags.txt" );

                using ( var webClient = new HttpClient() )
                {
                    WikiTaglist = await webClient.GetStringAsync( wikiUrl );
                }

                using ( StreamWriter sw = new StreamWriter( "TagsToDo.txt" ) )
                {
                    // If you wish to check duplicates uncomment next line
                    //SearchDuplicates( sw, WikiTaglist );
                    SearchMissingInWiki( sw, ExistingTags, WikiTaglist );
                }
            }
            catch ( Exception e )
            {
                Console.WriteLine( $"CheckTaglist Exception occurred: {e.Message}" );
                Console.WriteLine( $"CheckTaglist Exiting" );
            }

            Console.WriteLine( "\nPlease press any key to continue..." );
            Console.ReadKey();
        }
        static void SearchMissingInWiki( StreamWriter sw, string[] ExistingTags, string WikiTaglist )
        {
            int count = 0;

            sw.WriteLine( $"Missing the following tags in the Full List in the CumulusMX Wiki:\n" );

            foreach ( string TagName in ExistingTags )
            {
                if ( !WikiTaglist.Contains( $"&lt;#{TagName}" ) )
                {
                    if ( TagName.Contains( "AirLink" ) ) continue;                  // known to be there but complex because of [IN|OUT] addition
                    if ( char.IsDigit( TagName[ TagName.Length - 1 ] ) ) 
                        continue;  // Skip all Extra sensor tags (ending with a digit)

                    sw.WriteLine( $"Missing {TagName} in Wiki." );
                    count++;
                }
            }

            sw.WriteLine( $"\nMissing {count} TagName descriptions in Wiki.\n\n" );
            Console.WriteLine( $"Missing {count} TagName descriptions in Wiki." );

        }

        static void SearchDuplicates( StreamWriter sw, string WikiTaglist )
        {
            int count = 0, TagStartsAt, TagEndsAt=0, TagLength;

            List<int> Duplicates;
            List<string> DuplicateNames = new List<string>();

            sw.WriteLine( $"Duplicate tags in the Full List in the CumulusMX Wiki:\n" );

            do
            {
                string tmp;

                TagStartsAt = WikiTaglist.IndexOf( "&lt;#", TagEndsAt );
                if ( TagStartsAt == -1 ) break;

                TagEndsAt = WikiTaglist.IndexOf( "&gt", TagStartsAt );
                if ( TagEndsAt == -1 )
                {
                    // No closing > found => error
                    sw.WriteLine( $"Error in tagname {WikiTaglist.Substring( TagStartsAt, 30 )} in Wiki." );
                    Console.Write( $"Error in tagname {WikiTaglist.Substring( TagStartsAt, 30 )} in Wiki." );

                    break;
                }
                else TagEndsAt += 3;

                TagLength = TagEndsAt - TagStartsAt;
                tmp = WikiTaglist.Substring( TagStartsAt, TagLength );

                if ( tmp.Contains( "AirLink" ) ) continue;                  // known to be there but complex because of [IN|OUT] addition
                if ( char.IsDigit( tmp[ tmp.Length - 4 ] ) ) continue;      // Skip all Extra sensor tags (ending with a digit)

                if ( DuplicateNames.Contains( tmp ) ) continue;
                else Duplicates = AllIndexesOf(WikiTaglist, tmp);

                if (Duplicates.Count > 1)
                {
                    //    // Found a duplicate
                    int tmpLength = tmp.Length - 7;

                    sw.WriteLine( $"Found {Duplicates.Count} duplicates for {tmp.Substring( 4, tmpLength)} in Wiki." );
                    Console.WriteLine( $"Found {Duplicates.Count} duplicates for {tmp.Substring( 4, tmpLength )} in Wiki." );

                    DuplicateNames.Add( tmp );
                    TagEndsAt = TagStartsAt + TagLength;

                    count++;
                }
            } while ( TagEndsAt < WikiTaglist.Length );

            sw.WriteLine( $"\nFound {count} duplicate tagNames in Wiki.\n\n" );
            Console.WriteLine( $"Found {count} duplicate tagNames in Wiki.\n" );
        }

        static List<int> AllIndexesOf( string thisString, string thisSubstring )
        {
            // https://stackoverflow.com/questions/2641326/finding-all-positions-of-substring-in-a-larger-string-in-c-sharp

            List<int> indexes = new List<int>();

            for ( int index = 0; ; index += thisSubstring.Length )
            {
                index = thisString.IndexOf( thisSubstring, index );

                if ( index == -1 ) return indexes;
                else indexes.Add( index );
            }
        }
    }
}
