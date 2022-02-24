using TagLib;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using Microsoft.Data.Sqlite;

namespace music_library
{
    class DirectoryHelper
    {

        public DirectoryHelper()
        {

        }

        public string EmptyStringCheck(string x)
        {
            if(String.IsNullOrEmpty(x)) return String.Empty;
            else return x;
        }

        public void ImportLibrary(string path)
        {
            // Local constants

            // Local variables

            /****** start ImportLibrary() ******/

            var ext = new List<string> {"aac","aiff","ape","au","flac","gsm","it","m3u","m4a","mid","mod","mp3","mpa","pls","ra","s3m","sid","wav","wma","xm"};
            /*var files = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(s => SupportedMimeType.AllExtensions.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));*/

            var files = Directory
                .EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(s => ext.Contains(Path.GetExtension(s).TrimStart('.').ToLowerInvariant()));

            foreach(var file in files)
            {
                string artist_id = "";
                string album_id = "";
                var tfile = TagLib.File.Create(file);

                // Write to DB
                using (var connection = new SqliteConnection("Data Source=music_library.db"))
                {
                    connection.Open();

                    // Check to see if artist exists
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT id
                        FROM artist
                        WHERE name = '$name';
                    ".Replace("$name", tfile.Tag.JoinedAlbumArtists);

                    using(var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            artist_id = reader.GetString(0);
                        }
                    }

                    if(String.IsNullOrEmpty(artist_id))
                    {
                        artist_id = Guid.NewGuid().ToString();

                        command = connection.CreateCommand();
                        command.CommandText = 
                        @"
                            INSERT INTO artist VALUES($id, $name);
                        ";
                        command.Parameters.AddWithValue("$id", artist_id);
                        command.Parameters.AddWithValue("$name", EmptyStringCheck(tfile.Tag.JoinedAlbumArtists));
                        command.ExecuteNonQuery();
                    }

                    // Check to see if album exists
                    command = connection.CreateCommand();
                    command.CommandText =
                    @"
                        SELECT id
                        FROM album
                        WHERE name = $name
                        AND artist_id = $artist_id
                        AND year = $year
                        AND total_tracks = $total_tracks;
                    ";
                    command.Parameters.AddWithValue("$name", tfile.Tag.Album);
                    command.Parameters.AddWithValue("$artist_id", artist_id);
                    command.Parameters.AddWithValue("$year", tfile.Tag.Year);
                    command.Parameters.AddWithValue("$total_tracks", tfile.Tag.TrackCount);

                    using(var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            album_id = reader.GetString(0);
                        }
                    }

                    if(String.IsNullOrEmpty(album_id))
                    {
                        album_id = Guid.NewGuid().ToString();

                        command = connection.CreateCommand();
                        command.CommandText = 
                        @"
                            INSERT INTO album VALUES($id, $artist_id, $name, $genre, $year, $total_tracks, $total_discs);
                        ";
                        command.Parameters.AddWithValue("$id", EmptyStringCheck(album_id));
                        command.Parameters.AddWithValue("$artist_id", EmptyStringCheck(artist_id));
                        command.Parameters.AddWithValue("$name", EmptyStringCheck(tfile.Tag.Album));
                        command.Parameters.AddWithValue("$genre", EmptyStringCheck(tfile.Tag.JoinedGenres));
                        command.Parameters.AddWithValue("$year", tfile.Tag.Year);
                        command.Parameters.AddWithValue("$total_tracks", tfile.Tag.TrackCount);
                        command.Parameters.AddWithValue("$total_discs", tfile.Tag.DiscCount);
                        command.ExecuteNonQuery();
                    }

                    Console.WriteLine("Adding " + tfile.Tag.JoinedAlbumArtists + " - " + tfile.Tag.Album + " - " + tfile.Tag.Title);
                    var track_id = Guid.NewGuid().ToString();
                    command = connection.CreateCommand();
                    command.CommandText = 
                    @"
                        INSERT INTO track VALUES($id, $album_id, $artist_id, $title, $composer, $grouping, $track_number, $disc_number, $bpm, $comments, $duration);
                    ";
                    command.Parameters.AddWithValue("$id", EmptyStringCheck(track_id));
                    command.Parameters.AddWithValue("$album_id", EmptyStringCheck(album_id));
                    command.Parameters.AddWithValue("$artist_id", EmptyStringCheck(artist_id));
                    command.Parameters.AddWithValue("$title", EmptyStringCheck(tfile.Tag.Title));
                    command.Parameters.AddWithValue("$composer", EmptyStringCheck(tfile.Tag.JoinedComposers));
                    command.Parameters.AddWithValue("$grouping", EmptyStringCheck(tfile.Tag.Grouping));
                    command.Parameters.AddWithValue("$track_number", tfile.Tag.Track);
                    command.Parameters.AddWithValue("$disc_number", tfile.Tag.Disc);
                    command.Parameters.AddWithValue("$bpm", tfile.Tag.BeatsPerMinute);
                    command.Parameters.AddWithValue("$comments", EmptyStringCheck(tfile.Tag.Comment));
                    command.Parameters.AddWithValue("$duration", tfile.Properties.Duration);
                    command.ExecuteNonQuery();


                    connection.Close();
                }


                
                /*Console.WriteLine(tfile.Tag.Title);         
                Console.WriteLine(tfile.Tag.Artist);
                Console.WriteLine(tfile.Tag.Album);
                Console.WriteLine(tfile.Tag.JoinedAlbumFirstArtist);
                Console.WriteLine(tfile.Tag.JoinedComposers);
                Console.WriteLine(tfile.Tag.Grouping);
                Console.WriteLine(tfile.Tag.JoinedGenres);
                Console.WriteLine(tfile.Tag.Year);
                Console.WriteLine(tfile.Tag.Track);
                Console.WriteLine(tfile.Tag.TrackCount);
                Console.WriteLine(tfile.Tag.Disc);
                Console.WriteLine(tfile.Tag.DiscCount);
                Console.WriteLine(tfile.Tag.BeatsPerMinute);
                Console.WriteLine(tfile.Tag.Comment);
            }

            using (var connection = new SqliteConnection("Data Source=library.db"))
            {
                connection.Open();
                connection.Close();
            }*/
            }
        }
    }
}
