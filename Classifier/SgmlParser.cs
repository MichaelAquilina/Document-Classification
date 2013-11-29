using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DocumentClassification
{
    public class SgmlParser
    {
        #region Global Variables
        public static int BufferSize = 8 * 1024;

        /// <summary>
        /// Attributes obtained from parsing. Key has the format TagName_AttributeName
        /// </summary>
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();

        public String HeadLine;
        public String DocID;
        public String DateLine;

        /// <summary>
        /// Parsing based variables used for parsing tokens inside the document
        /// </summary>
        private Stack<String> tagStack = new Stack<String>();   //stack used to hold the current tag state
        private StringBuilder builder = new StringBuilder();    //builder used to build tag strings and tokens
        private char c;

        /// <summary>
        /// File Based variables, used for reading file data and placing it into a buffer.
        /// </summary>
        private FileStream stream;                      //filestream object used to read data
        private byte[] buffer = new byte[BufferSize];   //buffer used to hold data at each read
        private int noBytes;                            //number of bytes last read from the file stream
        private int bufferIndex;                        //current position in the buffer
        private long filePosition;                       //current position in the file (in terms of bytes)
        private long documentPosition;                   //starting position of the document in the file
        private byte backBuffer;
        private long fileLength;

        public long FilePosition
        {
            get
            {
                return filePosition;
            }
            set
            {
                stream.Seek(value, 0);
                filePosition = value;
                FillBuffer();
                tagStack.Clear();
                ParseHead();    //for now assumes that the new position is the start of a new document, might need to be changed
            }
        }
        public long FileLength
        {
            get
            {
                return fileLength;
            } 
        }
        public long DocumentPosition
        {
            get { return documentPosition; }
        }
        
        #endregion

        public SgmlParser(String DocumentPath)
        {
            stream = new FileStream(DocumentPath, FileMode.Open);
            fileLength = stream.Length;
            filePosition = 0;
            bufferIndex = 0;
            FillBuffer();
            ParseHead();
        }

        public String Next()
        {
            while (filePosition < stream.Length)
            {
                c = NextChar();
                if (!isWhiteSpace(c))
                {
                    switch (c)
                    {
                        case '<': ParseTag(); break;    //parse tag;
                        default: return ParseToken(true);
                    }
                }
            }

            return null;
        }

        public String NextParagraph()
        {
            StringBuilder builder = new StringBuilder();
            bool par = false;
            while (filePosition < stream.Length)
            {
                c = NextChar();
                switch (c)
                {
                    case '<': if (ParseTag() == "P")
                                {
                                    if (par)
                                        return builder.ToString();          //parse tag;
                                    else
                                        par = true;
                                }      
                                break;
                    default: if (par) builder.Append(c);
                                break;    //add character to the paragraph
                }
            }

            return null;
        }

        public void Close()
        {
            stream.Close();
        }

        #region Private Methods

        private void ParseHead()
        {
            documentPosition = filePosition;
            Dictionary<string, StringBuilder> Content = new Dictionary<string, StringBuilder>();  //used to store values temporarily

            //parse the head of the sgml file
            while (filePosition < stream.Length)
            {
                c = NextChar();

                if (!isWhiteSpace(c))
                {
                    switch (c)
                    {
                        case '<': ParseTag(); break;    //parse tag;
                        default: if (!Content.ContainsKey(tagStack.Peek()))
                                Content[tagStack.Peek()] = new StringBuilder();
                                Content[tagStack.Peek()].Append(ParseToken(false));
                                Content[tagStack.Peek()].Append(' ');
                            break;
                    }
                }
                //head has been parsed if TEXT tag is reached
                if (tagStack.Count > 0 && tagStack.Peek() == "TEXT")
                    break;
            }

            DocID = Attributes["DOC_id"];
            if( Content.ContainsKey("DATELINE") )
                DateLine = Content["DATELINE"].ToString();
            if( Content.ContainsKey("HEADLINE") )
                HeadLine = Content["HEADLINE"].ToString();
        }

        private void ParseAttribute()
        {
            builder.Clear();
            String Attribute;
            String Value;

            while (!isWhiteSpace(c) && c != '=')
            {
                builder.Append(c);
                c = NextChar();
            }
            Attribute = builder.ToString();

            while (c != '=') c = NextChar();
            while (c != '"') c = NextChar();

            builder.Clear();
            while ( (c=NextChar()) != '"')
                builder.Append(c);

            Value = builder.ToString();
            Attributes[tagStack.Peek() + "_" + Attribute] = Value;      //store the attribute value
        }

        private String ParseTag()
        {
            builder.Clear();
            bool isClosingTag = false;

            if (NextChar() == '/')
                isClosingTag = true;
            else
                RollBack();

            //build the name of the tag
            while (!isWhiteSpace(c = NextChar()) && c!='>' && c!='/' )
                builder.Append(c);

            String Tag = builder.ToString();
            builder.Clear();

            if (isClosingTag)
                tagStack.Pop();
            else
                tagStack.Push(Tag);

            while (c != '>')
            {
                if( !isWhiteSpace(c) )
                {
                    switch(c)
                    {
                        case '/' : tagStack.Pop(); break;
                        default : ParseAttribute(); break;
                    }
                }

                c = NextChar();
            }

            if (tagStack.Count == 0)
                ParseHead();            //should fire an event handler at this stage for optimisation

            return Tag;
        }

        private String ParseToken( bool UseNumberConflation )
        {
            StringBuilder builder = new StringBuilder(30);
            //builder.Clear();
            bool hasNumber = false;
            while (filePosition < stream.Length)
            {
                if (isWhiteSpace(c) )
                {
                    if (hasNumber)
                    {
                        builder.Append("NUMBER");
                        hasNumber = false;
                    }

                    if (builder.Length > 0)
                        return builder.ToString();
                }
                else
                if (c == '<')   //THIS CASE IS RARE BUT IS INCLUDED JUST IN CASE
                {
                    RollBack();
                    return builder.ToString();
                }
                else
                if ( UseNumberConflation && isNumber(c))
                {
                    hasNumber = true;
                }
                else
                if (SuffixNode.AcceptedCharacter(c))
                {
                    if (hasNumber)
                    {
                        builder.Append("NUMBER");
                        hasNumber = false;
                    }

                    builder.Append(c);
                }

                c = NextChar();
            }

            return builder.ToString();
        }

        private char NextChar()
        {
            char c = (char) ((bufferIndex>=0)? buffer[bufferIndex] : backBuffer );
            
            bufferIndex++;
            filePosition++;

            if (bufferIndex >= BufferSize)
                FillBuffer();

            return c;
        }

        private bool isNumber(char c)
        {
            return (c >= '0') && (c <= '9');
        }

        private void RollBack()
        {
            bufferIndex--;
            filePosition--;
        }

        private void FillBuffer()
        {
            backBuffer = buffer[BufferSize - 1];
            noBytes = stream.Read(buffer, 0, BufferSize);
            bufferIndex = 0;
        }

        private bool isWhiteSpace(char c)
        {
            return (c == ' ') || (c == '\t') || (c == '\n') || (c == '\r' ) || (c=='-');
        }

        #endregion
    }
}
