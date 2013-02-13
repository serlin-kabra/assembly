﻿/* Copyright 2012 Aaron Dierking, TJ Tunnell, Jordan Mueller, Alex Reed
 * 
 * This file is part of ExtryzeDLL.
 * 
 * Extryze is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * Extryze is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with ExtryzeDLL.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExtryzeDLL.Blam.ThirdGen;
using ExtryzeDLL.Blam.ThirdGen.Structures;
using ExtryzeDLL.IO;
using ExtryzeDLL.Flexibility;
using ExtryzeDLL.Util;
using ExtryzeDLL.Blam.Util;

namespace ExtryzeDLL.Blam.ThirdGen.Structures
{
    public class ThirdGenTagTable
    {
        private List<ITagClass> _classes;
        private List<ITag> _tags;

        public ThirdGenTagTable(IReader reader, StructureValueCollection headerValues, MetaAddressConverter converter, BuildInformation buildInfo)
        {
            Load(reader, headerValues, converter, buildInfo);
        }

        public IList<ITagClass> Classes
        {
            get { return _classes.AsReadOnly(); }
        }

        public IList<ITag> Tags
        {
            get { return _tags.AsReadOnly(); }
        }

        private void Load(IReader reader, StructureValueCollection values, MetaAddressConverter converter, BuildInformation buildInfo)
        {
            if (values.GetNumber("magic") != CharConstant.FromString("tags"))
                throw new ArgumentException("Invalid index table header magic");

            // Classes
            int numClasses = (int)values.GetNumber("number of classes");
            Pointer classTableLocation = new Pointer(values.GetNumber("class table address"), converter);
            _classes = ReadClasses(reader, classTableLocation, numClasses, buildInfo);

            // Tags
            int numTags = (int)values.GetNumber("number of tags");
            Pointer tagTableLocation = new Pointer(values.GetNumber("tag table address"), converter);
            _tags = ReadTags(reader, tagTableLocation, numTags, buildInfo, converter);
        }

        private List<ITagClass> ReadClasses(IReader reader, Pointer classTableLocation, int numClasses, BuildInformation buildInfo)
        {
            StructureLayout layout = buildInfo.GetLayout("class entry");

            List<ITagClass> result = new List<ITagClass>();
            reader.SeekTo(classTableLocation.AsOffset());
            for (int i = 0; i < numClasses; i++)
            {
                StructureValueCollection values = StructureReader.ReadStructure(reader, layout);
                result.Add(new ThirdGenTagClass(values));
            }
            return result;
        }

        private List<ITag> ReadTags(IReader reader, Pointer tagTableLocation, int numTags, BuildInformation buildInfo, MetaAddressConverter converter)
        {
            StructureLayout layout = buildInfo.GetLayout("tag entry");

            List<ITag> result = new List<ITag>();
            reader.SeekTo(tagTableLocation.AsOffset());
            for (int i = 0; i < numTags; i++)
            {
                StructureValueCollection values = StructureReader.ReadStructure(reader, layout);
                result.Add(new ThirdGenTag(values, (ushort)i, converter, _classes));
            }
            return result;
        }
    }
}