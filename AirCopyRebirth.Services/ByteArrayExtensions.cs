﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirCopyRebirth.Services {
    // adapted from https://stackoverflow.com/questions/283456/byte-array-pattern-search
    public static class ByteArrayExtensions {

        public static bool Contains(this byte[] self, byte[] patternToFind, int offset = 0) {
            if (EitherArrayIsEmpty(self, patternToFind)) {
                return false;
            }
            for (int i = offset; i < self.Length; i++) {
                if (IsMatch(self, i, patternToFind))
                    return true;
                else { continue; }
            }
            return false;
        }

        static readonly int[] Empty = new int[0];
        public static int[] Locate(this byte[] self, byte[] patternToFind, int offset = 0) {
            if (EitherArrayIsEmpty(self, patternToFind))
                return Empty;

            var list = new List<int>();

            for (int i = offset; i < self.Length; i++) {
                if (!IsMatch(self, i, patternToFind))
                    continue;

                list.Add(i);
            }

            return list.Count == 0 ? Empty : list.ToArray();
        }

        static bool IsMatch(byte[] array, int position, byte[] patternToFind) {
            if (patternToFind.Length > (array.Length - position))
                return false;

            for (int i = 0; i < patternToFind.Length; i++)
                if (array[position + i] != patternToFind[i])
                    return false;

            return true;
        }

        static bool EitherArrayIsEmpty(byte[] array, byte[] patternToFind) {
            return array == null
                || patternToFind == null
                || array.Length == 0
                || patternToFind.Length == 0
                || patternToFind.Length > array.Length;
        }

        public static byte[] StripEndMarker(this byte[] self, byte[] patternToFind) {
            var patternLocations = self.Locate(patternToFind);
            if (patternLocations.Length == 0) { return self; }

            // the pattern should only appear once (at the end):
            int sizeOfArrayBeforePattern = patternLocations[0];
            byte[] strippedArray = new byte[sizeOfArrayBeforePattern];
            Array.Copy(self, strippedArray, sizeOfArrayBeforePattern);
            return strippedArray;
        }
    }
}
