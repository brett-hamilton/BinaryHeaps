///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Project:           Project 4 - Binary Heaps
//	File Name:         BinaryHeapDriver.cs
//	Description:       Takes an unsorted binary file and uses heaps to sort the file and produce a final sorted binary
//						file. Creates temporary binary files for merging and sorting.
//	Course:            CSCI 3230 - Algorithms	
//	Author:            Brett Hamilton, hamiltonb@etsu.edu, Dept. of Computing, East Tennessee State University
//	Created:           Sunday, April 18, 2021
//	Copyright:         Brett Hamilton, 2021
//
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BinaryHeaps
{
	#region Driver
	/// <summary>
	/// Handles the user-input variables and runs the sorting algorithm.
	/// </summary>
	public class BinaryHeapDriver
	{
		#region Main
		/// <summary>
		/// The main entry point for the program.
		/// </summary>
		/// <param name="args">Optional command-line arguments.</param>
		public static void Main (string[] args)
		{
			bool TESTING = false;									// For testing - automatically supplies 10mil file, 9999 heap, k = 7

			string stringFile;                                      // Holds the user input file name
			int heapSize;                                           // Holds the user input size of the heap
			int numToMerge;                                         // Holds the user input number of files to merge at once
			List<string> tempFiles;                                 // Holds the names of temp files created

			Stopwatch sw = Stopwatch.StartNew ( );					// Creates sw and starts stopwatch

			if (TESTING)
			{
				stringFile = "10000000.bin";						// Use 10 million file
				heapSize = 9999;									// Heap size for this test is 9999
				tempFiles = ParseBinaryFile (stringFile, heapSize);	// Break up the binary file into temps using heaps
				numToMerge = 7;										// Merge 7 files at a time

				Console.WriteLine ("Binary File = " + stringFile);
				Console.WriteLine ("  Heap Size = " + heapSize);
				Console.WriteLine ("          k = " + numToMerge);
			}
			else
			{
				Console.Write ("ENTER binary file to sort: ");
				stringFile = Console.ReadLine ( );
				Console.Write ("ENTER heap size: ");
				heapSize = Int32.Parse (Console.ReadLine ( ));

				tempFiles = ParseBinaryFile (stringFile, heapSize);	// Break up the binary file into temps using heaps

				Console.Write ("ENTER number of files to merge at a time: ");
				numToMerge = Int32.Parse (Console.ReadLine ( ));
			}

			List<string> finalFile = MergeFiles (tempFiles, numToMerge, tempFiles.Count + 1);	// Return one sorted binary file

			CleanUpTemps (finalFile[0]);							// Delete all temp files except final sorted one

			sw.Stop ( );											// Stops stopwatch
			Console.WriteLine ("Time used: {0} secs", sw.Elapsed.TotalMilliseconds / 1000);

			if (TESTING)											// Stop program to view time
				Console.ReadLine ( );

		} // end Main (string[])
		#endregion

		#region Delete Temp Files
		/// <summary>
		/// Deletes all temporary files the program created except the final sorted one.
		/// </summary>
		/// <param name="keep">Name of the final sorted binary file to keep.</param>
		private static void CleanUpTemps (string keep)
		{
			int numberOfFiles = Int32.Parse (Regex.Match (keep, @"\d+").Value);	// Get the number of files created

			for (int i = 1; i < numberOfFiles; i++)
				File.Delete ("temp" + i + ".bin");								// Delete all files up until last

		} // end CleanUpTemps (string)
		#endregion

		#region Get Initial Temp Files
		/// <summary>
		/// Takes in a binary file, breaks it into specified heap sizes, and writes those heaps to temporary
		/// binary files.
		/// </summary>
		/// <param name="stringFile">The binary file to break up into temps.</param>
		/// <param name="size">The size of each heap.</param>
		/// <returns>A list of names of temp files that were created.</returns>
		private static List<string> ParseBinaryFile (string stringFile, int size)
		{
			int counter = 1;                                                    // Counter for making new temp file names
			List<string> temps = new List<string> ( );                          // Holds the list of all temp files created
			BinaryFile inFile = new BinaryFile (stringFile, false);             // Open the binary file for reading

			while (inFile.GetPosition ( ) != inFile.GetLength ( ))
			{
				string fileName = "temp" + counter + ".bin";                    // Create temp file name
				counter++;                                                      // Increment counter since this file name used
				BinaryFile outFile = new BinaryFile (fileName, true);           // Create temp file for writing
				temps.Add (fileName);                                           // Add temp file name to list

				MaxHeap heap = new MaxHeap (size);              // Create new heap

				for (int i = 0; (i < size) && (inFile.GetPosition ( ) != inFile.GetLength ( )); i++)
				{
					heap.Insert (BitConverter.ToInt32(inFile.Read (4), 0));		// Insert into heap
				}

				heap.HeapSort ( );                              // Sort the integer heap
				int[] arr = heap.GetInternalArray ( );          // Get the sorted internal array

				for (int i = 1; i < arr.Length; i++)                      // Write the integers to the temp file
					outFile.Write (arr[i]);
				
				outFile.Close ( );                              // Close the file we wrote to
			}

			inFile.Close ( );                                   // Close the file we read from

			return temps;

		} // end ParseBinaryFile (string, int)
		#endregion

		#region Merge Binary Files
		/// <summary>
		/// Recursive method that merges a list of binary files together using a min heap.
		/// </summary>
		/// <param name="files">The list of file names that need to be merged.</param>
		/// <param name="k">The number of files to merge together at once.</param>
		/// <param name="fileCounter">Indexer for the temp file string names.</param>
		/// <returns></returns>
		private static List<string> MergeFiles (List<string> files, int k, int fileCounter)
		{
			if (files.Count == 1)									// We have reached the final sorted file
				return files;

			int numEvenTimes = files.Count / k;						// How many times we can evenly merge the list of files by k
			int remainder = files.Count - (numEvenTimes * k);		// Get remainder that does not divide evenly
			List<string> newTemps = new List<string> ( );			// Holds names of new files we are creating
			MinHeap heap;											// Used to hold the heap of binary structs

			for (int i = 0; i < numEvenTimes * k; i += k)
			{
				BinStruct[] arr = new BinStruct[k];                 // Create array on BinStructs of size k
				int arrIndex = 0;									// Index for array of BinStructs
				for (int j = i; j < i + k; j++)
				{
					BinaryFile file = new BinaryFile (files[j], false);		// Create new binary file
					arr[arrIndex] = new BinStruct (file, BitConverter.ToInt32(file.Read (4), 0));	// Add binary struct to array of structs
					arrIndex++;										// Increment indexer for array
				}

				heap = new MinHeap (k);								// Create the min heap

				foreach (BinStruct b in arr)						// Fill the min heap
					heap.Insert (b);

				string newFileName = "temp" + fileCounter + ".bin";			// Create new file name
				fileCounter++;												// Increment file counter
				BinaryFile newTemp = new BinaryFile (newFileName, true);    // Create new temp file for merging
				newTemps.Add (newFileName);									// Add name of temp file to list of temp files created

				while (!heap.IsEmpty ( ))									// Until we reach end of heap
				{
					BinStruct extracted = heap.ExtractMin ( );				// Extract the min from the heap
					newTemp.Write (extracted.First);						// Write the minimum integer to the temp file

					// Check if we reached the end of the file
					if (extracted.BinFile.GetPosition ( ) < extracted.BinFile.GetLength ( ))
					{
						// Read next int in the file and assign it to the struct field
						extracted.First = BitConverter.ToInt32 (extracted.BinFile.Read (4), 0);
						heap.Insert (extracted);							// Insert the BinStruct back into the heap
					}
					else
					{
						extracted.BinFile.Close ( );						// We are finished with the file
					}
				}

				newTemp.Close ( );                                          // We are finished with this temp

				foreach (BinStruct b in arr)								// Make sure all files are closed
					b.BinFile.Close ( );
			}
			
			if (remainder == 1)
			{
				// If there is only one file remaining, nothing to merge - add it as is
				newTemps.Add (files[files.Count - 1]);
			}
			else if (remainder > 1)
			{
				BinStruct[] remainderArr = new BinStruct[remainder];        // Create array on BinStructs of size of remainder
				int remainderIndex = 0;										// Indexer for remainder array
				int startingIndex = files.Count - remainder;				// Index of first file to add to array
				for (int i = startingIndex; i < files.Count; i++)
				{
					BinaryFile file = new BinaryFile (files[i], false);		// Create a binary file
					remainderArr[remainderIndex] = new BinStruct (file, BitConverter.ToInt32(file.Read (4), 0));	// Create and add struct to array
					remainderIndex++;										// Increment array indexer
				}

				heap = new MinHeap (k);										// Create the min heap

				foreach (BinStruct b in remainderArr)						// Fill the min heap
					heap.Insert (b);

				string newFileName = "temp" + fileCounter + ".bin";			// Create new file name
				fileCounter++;												// Increment file counter
				BinaryFile newTemp = new BinaryFile (newFileName, true);	// Create new temp file for merging
				newTemps.Add (newFileName);									// Add name of temp file to list of temps

				while (!heap.IsEmpty ( ))									// Until we empty the heap
				{
					BinStruct extracted = heap.ExtractMin ( );				// Extract the minimum node from heap
					newTemp.Write (extracted.First);						// Write minimum int to the temp file

					if (extracted.BinFile.GetPosition ( ) == extracted.BinFile.GetLength ( ))
					{
						extracted.BinFile.Close ( );						// We are done with this file
						continue;
					}
					else
					{
						extracted.First = BitConverter.ToInt32 (extracted.BinFile.Read (4), 0);	// Read the next int into struct
						heap.Insert (extracted);							// Insert the struct back into the heap
					}
				}

				newTemp.Close ( );											// We are done with this temp file

				foreach (BinStruct b in remainderArr)						// Make sure all files are closed
					b.BinFile.Close ( );

			}

			return MergeFiles (newTemps, k, fileCounter);					// Recursive call until left with one final file

		} // end MergeFiles (List<string>, int)
		#endregion

	} // end BinaryHeapDriver
	#endregion

	#region Binary Struct
	/// <summary>
	/// A struct that holds a binary file and the integer that is first in the file.
	/// </summary>
	public struct BinStruct
	{
		#region Properties
		/// <summary>
		/// The Binary File holding the integers.
		/// </summary>
		public BinaryFile BinFile
		{
			get;
		}
		/// <summary>
		/// The current first integer in the file
		/// </summary>
		public int First
		{
			get;
			set;
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor that sets up the Binary File and integers properties
		/// </summary>
		/// <param name="file">The Binary File of integers.</param>
		/// <param name="first">The current first integer in the file.</param>
		public BinStruct (BinaryFile file, int first)
		{
			BinFile = file;
			First = first;

		} // end BinStruct (BinaryFile, int)
		#endregion

	} // end BinStruct
	#endregion

	#region Max Heap
	/// <summary>
	/// Represents and manages a max heap structure.
	/// </summary>
	internal class MaxHeap
	{
		#region Variables
		private int max_size;                       // The max size allowable for this heap
		private int size;                           // The current size of the heap
		private int arr_size;                       // The current size of the internal array
		private int[] h;                            // The array of integers representing the heap
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new heap of a given size and initializes the variables.
		/// </summary>
		/// <param name="size">The max size of the heap (+1).</param>
		public MaxHeap (int size)
		{
			this.size = 0;
			this.arr_size = 0;
			max_size = size + 1;                    // Add 1 to the size because we do not use index 0
			h = new int[max_size];                  // Creates the internal array (the "heap")
		} // end MaxHeap (int)
		#endregion

		#region Heap Management
		/// <summary>
		/// Increases the current size of the heap and inserts the new integer at the end.
		/// </summary>
		/// <param name="item">The integer to be inserted.</param>
		public void Insert (int item)
		{
			size++;                                 // Add 1 to current size
			arr_size++;                             // Add 1 to internal array size
			h[size] = item;                         // Add the integer to the end

			MakeMaxHeap (size);                     // Fix the heap after adding this new integer

		} // end Insert (string)

		/// <summary>
		/// Keep extracting the root node until the heap is empty.
		/// </summary>
		public void HeapSort ( )
		{
			while (size > 1)
			{
				// Extract the max until the heap is empty
				ExtractMax ( );
			}

		} // end HeapSort ( )

		/// <summary>
		/// Extract the root string (the max) by switching the root with the end string, then fix heap.
		/// </summary>
		private void ExtractMax ( )
		{
			if (size <= 1)                          // Stop if there is nothing left to extract in the heap
				return;

			// Swap the first item and the last item
			int temp = h[1];                     // Temporarily store the root string for swapping
			h[1] = h[size];
			h[size] = temp;

			size--;                                 // Decrement heap size

			FixMaxHeap (1);                         // Fix the heap

		} // end ExtractMax ( )

		/// <summary>
		/// Work down the heap, switching any misplaced strings to keep the max heap true.
		/// </summary>
		/// <param name="n">The current index of the string in the heap.</param>
		private void FixMaxHeap (int n)
		{
			int largestNode;                                    // Holds the index of the larger of the left or right child

			if (n > size)                                       // The index should never be > size of the heap
				return;
			else if ((n * 2) > size && (n * 2 + 1) > size)      // If this string has no children, return
				return;
			else if ((n * 2) <= size && (n * 2 + 1) > size)     // If there is no right child, must use left
				largestNode = n * 2;
			else if (h[n * 2] > h[n * 2 + 1])                   // If the left child is larger, use left
				largestNode = n * 2;
			else                                                // Right child is larger, use right
				largestNode = n * 2 + 1;

			// If the largest child is greater than the current parent string, swap them
			if (h[largestNode] > h[n])
			{
				int temp = h[n];                             // Temporarily store the current string for swapping
				h[n] = h[largestNode];
				h[largestNode] = temp;
			}

			FixMaxHeap (largestNode);                           // Recursive call, starting with the largest child index

		} // end FixMaxHeap (int)

		/// <summary>
		/// Work up the heap after a string has been inserted, making sure the max heap is true.
		/// </summary>
		/// <param name="n">The current index of the string in the heap.</param>
		private void MakeMaxHeap (int n)
		{

			if (n <= 1)
				return;
			else if (h[n / 2] < h[n])                               // If the parent string is smaller than this, swap it
			{
				int temp = h[n];                                 // Temporarily store this string for swapping
				h[n] = h[n / 2];
				h[n / 2] = temp;
			}

			MakeMaxHeap (n / 2);                                    // Recursive call, using the parent index

		} // end MakeMaxHeap (int)
		#endregion

		#region Printing & String Methods
		/// <summary>
		/// Prints the contents of the heap to the console.
		/// Example: a b c d e f g h i j
		/// </summary>
		public void PrintHeap ( )
		{
			for (int i = 1; i <= size; i++)
				Console.Write (h[i] + " ");
			Console.Write ("\n");                                   // End with a newline

		} // end PrintHeap ( )

		/// <summary>
		/// Prints the contents of the internal array to the console.
		/// Example:
		/// a
		/// b
		/// c
		/// d
		/// e
		/// </summary>
		public void PrintArray ( )
		{
			for (int i = 1; i < max_size; i++)
				Console.WriteLine (h[i]);

		} // end PrintArray ( )

		// Return the internal array
		public int[] GetInternalArray ( )
		{
			int[] arr = new int[arr_size + 1];                      // Hold internal array

			for (int i = 0; i < arr_size + 1; i++)
				arr[i] = h[i];

			return arr;

		} // end GetInternalArray ( )
		#endregion

	} // end MaxHeap
	#endregion

	#region Min Heap
	/// <summary>
	/// Represents and manages a min heap structure.
	/// </summary>
	internal class MinHeap
	{
		#region Variables
		private int max_size;                       // The max size allowable for this heap
		private int size;                           // The current size of the heap
		private int arr_size;                       // The current size of the internal array
		private bool is_empty;						// True if heap is empty
		private BinStruct[] h;                      // The array of integers representing the heap
		#endregion

		#region Constructors
		/// <summary>
		/// Creates a new heap of a given size and initializes the variables.
		/// </summary>
		/// <param name="size">The max size of the heap (+1).</param>
		public MinHeap (int size)
		{
			this.size = 0;
			this.arr_size = 0;
			this.is_empty = true;					// Initial heap is full
			max_size = size + 1;                    // Add 1 to the size because we do not use index 0
			h = new BinStruct[max_size];            // Creates the internal array (the "heap")
		} // end MaxHeap (int)
		#endregion

		#region Getters
		/// <summary>
		/// Getter for is_empty. Indicates whether there are any nodes in the heap.
		/// </summary>
		/// <returns>True if heap is empty, false otherwise.</returns>
		public bool IsEmpty ( )
		{
			return is_empty;
		} // end IsEmpty ( )
		#endregion

		#region Heap Management
		/// <summary>
		/// Increases the current size of the heap and inserts the new BinStruct at the end.
		/// </summary>
		/// <param name="item">The BinStruct to be inserted.</param>
		public void Insert (BinStruct item)
		{
			size++;                                 // Add 1 to current size
			is_empty = false;						// Heap not empty
			arr_size++;                             // Add 1 to internal array size
			h[size] = item;                         // Add the string to the end

			MakeMinHeap (size);                     // Fix the heap after adding this new BinStruct

		} // end Insert (string)

		/// <summary>
		/// Keep extracting the root node until the heap is empty.
		/// </summary>
		public void HeapSort ( )
		{
			while (size > 1)
			{
				// Extract the min until the heap is empty
				ExtractMin ( );
			}

		} // end HeapSort ( )

		/// <summary>
		/// Extract the root string (the min) by switching the root with the end BinStruct, then fix heap.
		/// </summary>
		public BinStruct ExtractMin ( )
		{
			// Swap the first item and the last item
			BinStruct temp = h[1];                  // Temporarily store the root BinStruct for swapping
			h[1] = h[size];
			h[size] = temp;

			size--;                                 // Decrement heap size

			if (size == 0)
				is_empty = true;					// Update if size is 0

			FixMinHeap (1);                         // Fix the heap

			return temp;							// Return the struct that was extracted (the min)

		} // end ExtractMax ( )

		/// <summary>
		/// Work down the heap, switching any misplaced BinStructs to keep the min heap true.
		/// </summary>
		/// <param name="n">The current index of the BinStruct in the heap.</param>
		private void FixMinHeap (int n)
		{
			int smallestNode;                                   // Holds the index of the smaller of the left or right child

			if (n > size)                                       // The index should never be > size of the heap
				return;
			else if ((n * 2) > size && (n * 2 + 1) > size)      // If this node has no children, return
				return;
			else if ((n * 2) <= size && (n * 2 + 1) > size)     // If there is no right child, must use left
				smallestNode = n * 2;
			else if (h[n * 2].First < h[n * 2 + 1].First)       // If the left child is smaller, use left
				smallestNode = n * 2;
			else                                                // Right child is smaller, use right
				smallestNode = n * 2 + 1;

			// If the smallest child is less than the current parent node, swap them
			if (h[smallestNode].First < h[n].First)
			{
				BinStruct temp = h[n];                          // Temporarily store the current BinStruct for swapping
				h[n] = h[smallestNode];
				h[smallestNode] = temp;
			}

			FixMinHeap (smallestNode);                          // Recursive call, starting with the smallest child index

		} // end FixMinHeap (int)

		/// <summary>
		/// Work up the heap after a node has been inserted, making sure the min heap is true.
		/// </summary>
		/// <param name="n">The current index of the BinStruct in the heap.</param>
		private void MakeMinHeap (int n)
		{

			if (n <= 1)
				return;
			else if (h[n / 2].First > h[n].First)                   // If the parent string is larger than this, swap it
			{
				BinStruct temp = h[n];                              // Temporarily store this node for swapping
				h[n] = h[n / 2];
				h[n / 2] = temp;
			}

			MakeMinHeap (n / 2);                                    // Recursive call, using the parent index

		} // end MakeMinHeap (int)
		#endregion

		#region Printing & String Methods
		/// <summary>
		/// Prints the contents of the heap to the console.
		/// Example: a b c d e f g h i j
		/// </summary>
		public void PrintHeap ( )
		{
			for (int i = 1; i <= size; i++)
				Console.Write (h[i] + " ");
			Console.Write ("\n");                                   // End with a newline

		} // end PrintHeap ( )

		/// <summary>
		/// Prints the contents of the internal array to the console.
		/// Example:
		/// a
		/// b
		/// c
		/// d
		/// e
		/// </summary>
		public void PrintArray ( )
		{
			for (int i = 1; i < max_size; i++)
				Console.WriteLine (h[i]);

		} // end PrintArray ( )

		/// <summary>
		/// Get the internal array of the heap.
		/// </summary>
		/// <returns>The internal array of the heap.</returns>
		public BinStruct[] GetInternalArray ( )
		{
			BinStruct[] arr = new BinStruct[arr_size];                    // Hold internal array

			for (int i = 0; i < arr_size; i++)
				arr[i] = h[i];

			return arr;

		} // end GetInternalArray ( )
		#endregion
	}
	#endregion
}