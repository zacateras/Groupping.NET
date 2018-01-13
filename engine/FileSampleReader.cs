using System;
using System.Collections.Generic;
using System.IO;

namespace Groupping.NET
{
    public class FileSampleReader
    {
        private readonly Random _random;
        private readonly string _path;
        private readonly bool _header;

        public FileSampleReader(string path, bool header = true)
        {
            _random = new Random();
            _path = path;
            _header = header;
        }

        public IEnumerable<string> ReadAbsoluteSample(int absolute)
        {
            int lines = CountLines();
            double chance = absolute / lines;

            return ReadSample(lines, chance);
        }

        public IEnumerable<string> ReadFractionSample(double fraction)
        {
            int lines = CountLines();
            double chance = fraction;

            return ReadSample(lines, chance);
        }

        private IEnumerable<string> ReadSample(int lines, double chance)
        {
            using (var reader = new StreamReader(_path))
            {
                reader.BaseStream.Seek(
                    _header ? 1 : 0,
                    System.IO.SeekOrigin.Begin);

                while (lines > 0)
                {
                    string line = reader.ReadLine();

                    if (line == null)
                    {
                        reader.DiscardBufferedData();
                        reader.BaseStream.Seek(
                            _header ? 1 : 0,
                            System.IO.SeekOrigin.Begin);
                    }

                    if (_random.NextDouble() < chance)
                    {
                        --lines;
                        yield return line;
                    }
                }
            }
        }

        private int CountLines()
        {
            using (var streamReader = new StreamReader(_path))
            {
                int i = 0;
                while (streamReader.ReadLine() != null) ++i;
                return i;
            }
        }
    }
}