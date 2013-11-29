using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;

namespace HLTAssignment
{
    class GUITextWriter : TextWriter
    {
        private StringBuilder builder = new StringBuilder();
        private TextBox textbox;

        public override Encoding Encoding
        {
            get { return Encoding.UTF8;  }
        }

        public GUITextWriter(TextBox text)
        {
            this.textbox = text;
        }

        public override void Write(string value)
        {
            builder.Append(value);
            textbox.Text = builder.ToString();
        }

        public override void WriteLine( string value )
        {
            builder.Append(value);
            builder.Append('\n');
            textbox.Text = builder.ToString();
        }
    }
}
