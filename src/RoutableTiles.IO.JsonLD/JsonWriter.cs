using System;
using System.Collections.Generic;
using System.IO;

namespace RoutableTiles.IO.JsonLD
{
    /// <summary>
    /// A json-writer.
    /// </summary>
    internal class JsonWriter : IDisposable
    {
        private readonly TextWriter _writer;
        private readonly Stack<Status> _statusStack;
        
        /// <summary>
        /// Creates a new json writer.
        /// </summary>
        internal JsonWriter(TextWriter writer)
        {
            _writer = writer;
            _statusStack = new Stack<Status>();
        }

        /// <summary>
        /// Writes the object open char.
        /// </summary>
        public void WriteOpen()
        {
            if (_statusStack.Count > 0)
            {
                var status = _statusStack.Peek();
                
                if (status == Status.ArrayValueWritten)
                {
                    _writer.Write(',');
                }
            }

            _statusStack.Push(Status.ObjectOpened);
            _writer.Write('{');
        }

        /// <summary>
        /// Writes the object close char.
        /// </summary>
        public void WriteClose()
        {
            Status status;
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot close object at this point.");
            }
            if (_statusStack.Count > 0)
            {
                status = _statusStack.Peek();

                if(status == Status.PropertyNameWritten)
                {
                    throw new Exception("Cannot close object right after writing a property name.");
                }
            }
            _writer.Write('}');
            while (_statusStack.Peek() != Status.ObjectOpened)
            {
                _statusStack.Pop();
            }
            _statusStack.Pop();

            if (_statusStack.Count <= 0) return;
            status = _statusStack.Peek();
            switch (status)
            {
                case Status.PropertyNameWritten: // the object was a property value.
                    _statusStack.Push(Status.PropertyValueWritten);
                    break;
                case Status.ArrayOpenWritten:
                case Status.ArrayValueWritten: // the array was an array value.
                    _statusStack.Push(Status.ArrayValueWritten);
                    break;
            }
        }

        /// <summary>
        /// Writes a property name.
        /// </summary>
        public void WritePropertyName(string name, bool escape = false)
        {
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot write property name at this point.");
            }
            else
            {
                var status = _statusStack.Peek();
                if (status != Status.PropertyValueWritten &&
                    status != Status.ObjectOpened)
                {
                    throw new Exception("Cannot write property name at this point.");
                }

                if (status == Status.PropertyValueWritten)
                { // write comma before starting new property.
                    _writer.Write(',');
                }
            }

            _writer.Write('"');
            if (escape)
            {
                name = JsonTools.Escape(name);
            }
            _writer.Write(name);
            _writer.Write('"');
            _writer.Write(':');
            _statusStack.Push(Status.PropertyNameWritten);
        }
        
        /// <summary>
        /// Writes a property value.
        /// </summary>
        public void WritePropertyValue(string value, bool useQuotes = false, bool escape = false)
        {
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot write property value at this point.");
            }
            else
            {
                var status = _statusStack.Peek();
                if (status != Status.PropertyNameWritten)
                {
                    throw new Exception("Cannot write property value at this point.");
                }
            }

            if (useQuotes) _writer.Write('"');
            if (escape)
            {
                value = JsonTools.Escape(value);
            }
            _writer.Write(value);
            if (useQuotes) _writer.Write('"');
            _statusStack.Push(Status.PropertyValueWritten);
        }

        /// <summary>
        /// Writes a property and it's value.
        /// </summary>
        public void WriteProperty(string name, string value, bool useQuotes = false, bool escape = false)
        {
            this.WritePropertyName(name, escape);
            this.WritePropertyValue(value, useQuotes, escape);
        }

        /// <summary>
        /// Writes the array open char.
        /// </summary>
        public void WriteArrayOpen()
        {
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot open array at this point.");
            }
            else
            {
                var status = _statusStack.Peek();
                if (status != Status.PropertyNameWritten &&
                    status != Status.ArrayOpenWritten &&
                    status != Status.ArrayValueWritten)
                {
                    throw new Exception("Cannot open array at this point.");
                }

                if (status == Status.ArrayValueWritten)
                {
                    _writer.Write(',');
                }
            }

            _writer.Write('[');
            _statusStack.Push(Status.ArrayOpenWritten);
        }

        /// <summary>
        /// Writes the array close char.
        /// </summary>
        public void WriteArrayClose()
        {
            Status status;
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot open array at this point.");
            }
            else
            {
                status = _statusStack.Peek();
                if (status != Status.ArrayOpenWritten &&
                    status != Status.ArrayValueWritten)
                {
                    throw new Exception("Cannot open array at this point.");
                }
            }

            _writer.Write(']');

            status = _statusStack.Peek();
            while(status != Status.ArrayOpenWritten)
            {
                _statusStack.Pop();
                status = _statusStack.Peek();
            }
            _statusStack.Pop();


            if (_statusStack.Count <= 0) return;
            status = _statusStack.Peek();
            switch (status)
            {
                case Status.PropertyNameWritten: // the array was a property value.
                    _statusStack.Push(Status.PropertyValueWritten);
                    break;
                case Status.ArrayOpenWritten:
                case Status.ArrayValueWritten: // the array was an array value.
                    _statusStack.Push(Status.ArrayValueWritten);
                    break;
            }
        }

        /// <summary>
        /// Writes an array value.
        /// </summary>
        public void WriteArrayValue(string value, bool useQuotes = false, bool escape = true)
        {
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot write array at this point.");
            }
            else
            {
                var status = _statusStack.Peek();
                if (status != Status.ArrayOpenWritten &&
                    status != Status.ArrayValueWritten)
                {
                    throw new Exception("Cannot write array at this point.");
                }

                if (status == Status.ArrayValueWritten)
                {
                    _writer.Write(",");
                }
            }
            
            if (useQuotes) _writer.Write('"');
            if (escape)
            {
                value = JsonTools.Escape(value);
            }
            _writer.Write(value);
            if (useQuotes) _writer.Write('"');
            _statusStack.Push(Status.ArrayValueWritten);
        }

        /// <summary>
        /// Gets the text writer.
        /// </summary>
        /// <returns></returns>
        public TextWriter TextWriter => _writer;

        private enum Status
        {
            ObjectOpened,
            PropertyNameWritten,
            PropertyValueWritten,
            ArrayOpenWritten,
            ArrayValueWritten
        }
        
        /// <summary>
        /// Flushes this writer.
        /// </summary>
        public void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// Releases all resources used by the json writer object.
        /// </summary>
        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}