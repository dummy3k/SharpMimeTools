// -----------------------------------------------------------------------
//
//   Copyright (C) 2003-2004 Angel Marin
// 
//   This file is part of SharpMimeTools
//
//   SharpMimeTools is free software; you can redistribute it and/or
//   modify it under the terms of the GNU Lesser General Public
//   License as published by the Free Software Foundation; either
//   version 2.1 of the License, or (at your option) any later version.
//
//   SharpMimeTools is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//   Lesser General Public License for more details.
//
//   You should have received a copy of the GNU Lesser General Public
//   License along with SharpMimeTools; if not, write to the Free Software
//   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
// -----------------------------------------------------------------------

using System;

namespace anmar.SharpMimeTools
{
	/// <summary>
	/// </summary>
	internal class SharpMimeMessageStream {
		private static log4net.ILog log  = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		protected System.IO.Stream stream;
		System.IO.StreamReader sr;
		System.Text.Encoding enc;
		protected long initpos;
		protected long finalpos;

		public SharpMimeMessageStream ( System.IO.Stream stream ) {
			this.stream = stream;
			this.enc = new System.Text.ASCIIEncoding();
			sr = new System.IO.StreamReader ( this.stream, this.enc );
		}
		public System.String ReadAll ( ) {
			return this.ReadLines ( this.Position, this.stream.Length );
		}
		public System.String ReadAll ( long start ) {
            return this.ReadLines ( start, this.stream.Length );
		}
		public System.String ReadLine ( ) {
			System.String line;
			this.initpos = this.Position;
			line = sr.ReadLine();
			if ( line!=null ) {
				this.finalpos=this.Position+this.enc.GetByteCount(line.ToCharArray())+this.enc.GetByteCount(new System.Char[]{'\r','\n'});
				if ( line.StartsWith(".." ) )
					line=line.Remove(0,1);
			} else {
				this.finalpos=this.stream.Length;
			}
			return line;
		}
		public System.String ReadLines ( long start, long end ) {
			return this.ReadLinesSB ( start, end ).ToString();
		}
		public System.Text.StringBuilder ReadLinesSB ( long start, long end ) {
			System.Text.StringBuilder lines = new System.Text.StringBuilder();
			System.String line;
			long initpos = this.Position;
			this.SeekPoint ( start );
			do {
				line = this.ReadLine();
				if ( line!=null ) {
					// TODO: try catch
					if ( lines.Length>0 )
						lines.Append ( ABNF.CRLF );
					lines.Append ( line );
				}
			} while ( line!=null && this.Position!=-1 && this.Position<end );
			this.initpos = initpos;
			return lines;            
		}
		public void ReadLine_Undo () {
			this.SeekPoint(this.initpos);
			this.finalpos = this.initpos;
		}
		public System.String ReadUnfoldedLine () {
			long initpos = this.Position;
			System.String  tmpline;
			System.Text.StringBuilder line = new System.Text.StringBuilder(72);
			tmpline = this.ReadLine();
			if ( tmpline!=null && tmpline.Length>0 ) {
				line.Append(tmpline);
				for ( ;;)  {
					tmpline = this.ReadLine();
					// RFC 2822 - 2.2.3 Long Header Fields
					if ( tmpline!=null && tmpline.Length>0 && (tmpline[0] == ' ' || tmpline[0] == '\t') ) {
						line = line.Append(tmpline, 0, tmpline.Length );
					} else {
						this.ReadLine_Undo();
						break;
					}
				}
				this.initpos = initpos;
			}
			return (this.finalpos!=this.initpos)?line.ToString():null;
		}
		public bool SeekLine ( long line ) {
			long linenumber = 0;
			this.SeekOrigin();
			for ( ; linenumber<(line-1) && this.ReadLine()!=null; linenumber++ ){}
			return (linenumber==(line-1))?true:false;
		}
		public void SeekOrigin () {
			this.SeekPoint (0);
		}
		public void SeekPoint ( long point ) {
			if ( this.sr.BaseStream.CanSeek && this.sr.BaseStream.Seek (point, System.IO.SeekOrigin.Begin) != point ) {
				if ( log.IsErrorEnabled) log.Error ("Error while seeking");
				throw new System.IO.IOException ();
			} else {
				this.sr.DiscardBufferedData();
				this.finalpos = point;
			}
		}
		public System.Text.Encoding Enconding {
			set {
				if ( value != null && this.enc!=value ) {
					this.enc = value;
					this.SeekPoint (this.Position);
					sr = new System.IO.StreamReader ( this.stream, this.enc );
				}
			}
		}
		public long Position {
			get { return this.finalpos; }
		}
		public long Position_preRead {
			get { return this.initpos; }
		}
		public System.IO.Stream Stream {
			get { return this.stream; }
		}
	}
}
