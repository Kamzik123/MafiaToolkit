﻿using System.IO;
using Mafia2Tool;

namespace Core.IO
{
    class FileTable : FileBase
    {
        public FileTable(FileInfo info) : base(info)
        {
        }

        public override string GetExtensionUpper()
        {
            return "TBL";
        }

        public override bool Open()
        {
            TableEditor editor = new TableEditor(file);
            return true;
        }
    }
}
