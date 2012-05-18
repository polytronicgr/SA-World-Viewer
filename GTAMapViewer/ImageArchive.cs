﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace GTAMapViewer
{
    public class ImageArchive
    {
        private struct ImageArchiveEntry
        {
            public readonly UInt32 Offset;
            public readonly UInt32 Size;
            public readonly String Name;

            public ImageArchiveEntry( Stream stream )
            {
                BinaryReader reader = new BinaryReader( stream );
                Offset = reader.ReadUInt32() << 11;
                ushort sizeSecond = reader.ReadUInt16();
                ushort sizeFirst = reader.ReadUInt16();
                Size = (UInt32) ( ( sizeFirst != 0 ) ? sizeFirst << 11 : sizeSecond << 11 );
                Name = new String( reader.ReadChars( 24 ) );

                for( int i = 0; i < 24; ++ i )
                {
                    if ( Name[ i ] == '\0' )
                    {
                        Name = Name.Substring( 0, i );
                        break;
                    }
                }
            }
        }

        public static ImageArchive Load( String filePath )
        {
            return new ImageArchive( new FileStream( filePath, FileMode.Open, FileAccess.Read ) );
        }

        private Stream myStream;
        private Dictionary<String, ImageArchiveEntry> myDict;

        public readonly String Version;
        public readonly UInt32 Length;

        public ImageArchive( Stream stream )
        {
            myStream = stream;

            BinaryReader reader = new BinaryReader( stream );
            Version = new String( reader.ReadChars( 4 ) );
            Length = reader.ReadUInt32();

            myDict = new Dictionary<string, ImageArchiveEntry>();

            for ( int i = 0; i < Length; ++i )
            {
                ImageArchiveEntry entry = new ImageArchiveEntry( stream );
                myDict.Add( entry.Name, entry );
            }
        }

        public GTAFileStream ReadFile( String name )
        {
            ImageArchiveEntry entry = myDict[ name ];
            return new GTAFileStream( myStream, entry.Offset, entry.Size );
        }
    }
}