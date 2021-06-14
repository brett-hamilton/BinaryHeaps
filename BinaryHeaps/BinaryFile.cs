///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Project:           Project 4 - Binary Heaps
//	File Name:         BinaryFile.cs
//	Description:       Modified class from binary files lab that helps manage binary file reading and writing.
//	Course:            CSCI 3230 - Algorithms	
//	Author:            Brett Hamilton, hamiltonb@etsu.edu, Dept. of Computing, East Tennessee State University
//	Created:           Sunday, April 18, 2021
//	Copyright:         Brett Hamilton, 2021
//
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryHeaps
{
    /// <summary>
    /// Provides methods and functionality relating to binary files.
    /// </summary>
	public class BinaryFile
	{
		#region Fields
		private BinaryWriter binOutFile;    // Output file
        private BinaryReader binInFile;     // Input file
        private long position = 0,          // Current file position
                 length = 0;                // Length of input file
        bool writing = false;               // Writing == true
		#endregion

		#region Constructor
        /// <summary>
        /// Open a file for writing or reading.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="writeit">If true, open BinaryWriter; if false, open BinaryReader.</param>
		public BinaryFile (string fileName, bool writeit)
        {
            writing = writeit; 
            if (writing)
            {
                // Open file as a writer
                binOutFile = new BinaryWriter (File.Open (fileName,
                          FileMode.Create, FileAccess.Write));
            }
            else
            {
                // Open file as a reader
                binInFile = new BinaryReader (File.Open (fileName,
                        FileMode.Open, FileAccess.Read));
                // Set the length of this file
                length = binInFile.BaseStream.Length;
            }

        } // end BinaryFile (string, bool)
		#endregion

		#region Supporting Methods
		/// <summary>
		/// Write a single integer to the file.
		/// </summary>
		/// <param name="buffer">The integer to write.</param>
		public void Write (int buffer)
        {
            binOutFile.Write (buffer);

        } // end Write (int)

        /// <summary>
        /// Read the next integer in the file.
        /// </summary>
        /// <param name="size">How many bytes to read.</param>
        /// <returns>The byte array representing an integer.</returns>
        public byte[] Read (int size)
        {
            byte[] buffer = new byte[size];             // Create the buffer for the bytes
            binInFile.Read (buffer, 0, size);           // Read in the bytes
            position += 4;                              // Increment the position in this file
            return (buffer);

        } // end Read (int)

        /// <summary>
        /// Return the length (in bytes) of this file.
        /// </summary>
        /// <returns>The length of the file.</returns>
        public long GetLength ( )
        {
            return length;

        } // end GetLength ( )

        /// <summary>
        /// Get the current offset into this file.
        /// </summary>
        /// <returns>The current offset position of the file.</returns>
        public long GetPosition ( )
		{
            return position;

		} // end GetPosition ( )

        /// <summary>
        /// Close the file.
        /// </summary>
        public void Close ( )
        {
            if (writing) binOutFile.Close ( );
            else binInFile.Close ( );

        } // Close ( )
		#endregion

	} // end BinaryFile
}
