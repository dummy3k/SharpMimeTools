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
	/// rfc 2822 header of a rfc 2045 entity
	/// </summary>
	public class SharpMimeHeader : System.Collections.IEnumerable {
		private static log4net.ILog log  = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private anmar.SharpMimeTools.SharpMimeMessageStream message;
		private System.Collections.Specialized.HybridDictionary headers;
		private long startpoint;
		private long endpoint;
		private long startbody;
		private System.Text.Encoding enc = new System.Text.ASCIIEncoding();

		private struct HeaderInfo {
			public System.Collections.Specialized.StringDictionary contenttype;
			public System.Collections.Specialized.StringDictionary contentdisposition;
			public System.Collections.Specialized.StringDictionary contentlocation;
			public anmar.SharpMimeTools.MimeTopLevelMediaType TopLevelMediaType;
			public System.Text.Encoding enc;
			public System.String subtype;

			public HeaderInfo ( System.Collections.Specialized.HybridDictionary headers ) {
				this.TopLevelMediaType = new anmar.SharpMimeTools.MimeTopLevelMediaType();
				this.enc = null;
				try {
					this.contenttype = anmar.SharpMimeTools.SharpMimeTools.parseHeaderFieldBody ( "Content-Type", headers["Content-Type"].ToString() );
					this.TopLevelMediaType = (anmar.SharpMimeTools.MimeTopLevelMediaType)System.Enum.Parse(TopLevelMediaType.GetType(), this.contenttype["Content-Type"].Split('/')[0], true);
					this.subtype = this.contenttype["Content-Type"].Split('/')[1];
					this.enc = anmar.SharpMimeTools.SharpMimeTools.parseCharSet ( this.contenttype["charset"] );
				} catch (System.Exception) {
					this.contenttype = anmar.SharpMimeTools.SharpMimeTools.parseHeaderFieldBody ( "Content-Type", "text/plain; charset=us-ascii" );
					this.TopLevelMediaType = anmar.SharpMimeTools.MimeTopLevelMediaType.text;
					this.subtype = "plain";
				}
				if ( this.enc==null ) {
					this.enc = new System.Text.ASCIIEncoding();
				}
				// TODO: rework this
				try {
					this.contentdisposition = anmar.SharpMimeTools.SharpMimeTools.parseHeaderFieldBody ( "Content-Disposition", headers["Content-Disposition"].ToString() );
				} catch ( System.Exception ) {
					this.contentdisposition = new System.Collections.Specialized.StringDictionary();
				}
				try {
					this.contentlocation = anmar.SharpMimeTools.SharpMimeTools.parseHeaderFieldBody ( "Content-Location", headers["Content-Location"].ToString() );
				} catch ( System.Exception ) {
					this.contentlocation = new System.Collections.Specialized.StringDictionary();
				}
			}
		}
		private HeaderInfo mt;

		internal SharpMimeHeader( anmar.SharpMimeTools.SharpMimeMessageStream message ) : this ( message, 0 ){}
		internal SharpMimeHeader(anmar.SharpMimeTools.SharpMimeMessageStream message, long startpoint) {
			this.startpoint = startpoint;
			this.message = message;
			if ( this.startpoint==0 ) {
				System.String line = this.message.ReadLine();
				// Perhaps there is part of the POP3 response
				if ( line!=null && line.StartsWith ("+OK") ) {
					if ( log.IsDebugEnabled ) log.Debug ("+OK present at top of the message");
					this.startpoint = this.message.Position;
				} else this.message.ReadLine_Undo();
			}
			this.headers = new System.Collections.Specialized.HybridDictionary(2, true);
			this.parse();
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpMimeHeader"/> class from a <see cref="System.IO.Stream"/>
		/// </summary>
		/// <param name="message"><see cref="System.IO.Stream"/> to read headers from</param>
		public SharpMimeHeader( System.IO.Stream message ) : this( new anmar.SharpMimeTools.SharpMimeMessageStream (message), 0 ) {
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpMimeHeader"/> class from a <see cref="System.IO.Stream"/> starting from the specified point
		/// </summary>
		/// <param name="message">the <see cref="System.IO.Stream" /> to read headers from</param>
		/// <param name="startpoint">initial point of the <see cref="System.IO.Stream"/> where the headers start</param>
		public SharpMimeHeader( System.IO.Stream message, long startpoint ) : this( new anmar.SharpMimeTools.SharpMimeMessageStream (message), startpoint ) {
		}
		/// <summary>
		/// Gets header fields
		/// </summary>
		/// <param name="name">field name</param>
		/// <remarks>Field names is case insentitive</remarks>
		public System.String this[ System.Object name ] {
			get {
				return this.getProperty( name.ToString() );
			}
		}
		/// <summary>
		/// Returns an enumerator that can iterate through the header fields
		/// </summary>
		/// <returns>A <see cref="System.Collections.IEnumerator" /> for the header fields</returns>
		public System.Collections.IEnumerator GetEnumerator() {
			return headers.GetEnumerator();
		}
		private System.String getProperty (  System.String name ) {
			System.String Value=null;
			name = name.ToLower();
			this.parse();
			if ( this.headers!=null && this.headers.Count > 0 && name!=null && name.Length>0 && this.headers.Contains(name) )
				Value = this.headers[name].ToString();
			return Value;
		}
		private bool parse () {
			bool error = false;
			if ( this.headers.Count>0 ) {
				return !error;
			}
			System.String line = System.String.Empty;
			this.message.SeekPoint ( this.startpoint );
			this.message.Enconding = this.enc;
			for ( line=this.message.ReadUnfoldedLine(); line!=null ; line=this.message.ReadUnfoldedLine() ) {
				if ( line.Length == 0 ) {
					this.endpoint = this.message.Position_preRead;
					this.startbody = this.message.Position;
					this.message.ReadLine_Undo();
					break;
				} else {
					String [] headerline = line.Split ( new Char[] {':'}, 2);
					if ( headerline.Length == 2 ) {
						headerline[1] = headerline[1].TrimStart(new Char[] {' '});
						if ( this.headers.Contains ( headerline[0]) ) {
							this.headers[headerline[0]] = this.headers[headerline[0]] + headerline[1];
						} else {
							this.headers.Add (headerline[0].ToLower(), headerline[1]);
						}
					}
				}
			}
			this.mt = new HeaderInfo ( this.headers );
			return !error;
		}
		/// <summary>
		/// Gets the point where the headers end
		/// </summary>
		/// <value>Point where the headers end</value>
		public long BodyPosition {
			get {
				return this.startbody;
			}
		}
		/// <summary>
		/// Gets CC header field
		/// </summary>
		/// <value>CC header body</value>
		public System.String Cc {
			get {
				System.String tmp = this.getProperty("Cc");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets the number of header fields found
		/// </summary>
		public int Count {
			get {
				return this.headers.Count;
			}
		}
		/// <summary>
		/// Gets Content-Disposition header field
		/// </summary>
		/// <value>Content-Disposition header body</value>
		public System.String ContentDisposition {
			get {
				System.String tmp = this.getProperty("Content-Disposition");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets the elements found in the Content-Disposition header body
		/// </summary>
		/// <value><see cref="System.Collections.Specialized.StringDictionary"/> with the elements found in the header body</value>
		public System.Collections.Specialized.StringDictionary ContentDispositionParameters {
			get {
				return this.mt.contentdisposition;
			}
		}
		/// <summary>
		/// Gets Content-Location header field
		/// </summary>
		/// <value>Content-Location header body</value>
		public System.String ContentLocation {
			get {
				System.String tmp = this.getProperty("Content-Location");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets the elements found in the Content-Location header body
		/// </summary>
		/// <value><see cref="System.Collections.Specialized.StringDictionary"/> with the elements found in the header body</value>
		public System.Collections.Specialized.StringDictionary ContentLocationParameters {
			get {
				return this.mt.contentlocation;
			}
		}
		/// <summary>
		/// Gets Content-Transfer-Encoding header field
		/// </summary>
		/// <value>Content-Transfer-Encoding header body</value>
		public System.String ContentTransferEncoding {
			get {
				System.String tmp = this.getProperty("Content-Transfer-Encoding");
				if ( tmp!=null ) {
					tmp = tmp.ToLower();
				}
				return tmp;
			}
		}
		/// <summary>
		/// Gets Content-Type header field
		/// </summary>
		/// <value>Content-Type header body</value>
		public System.String ContentType {
			get {
				System.String tmp = this.getProperty("Content-Type");
				if ( tmp==null ) {
					tmp = "text/plain; charset=us-ascii";
				}
				return tmp;
			}
		}
		/// <summary>
		/// Gets the elements found in the Content-Type header body
		/// </summary>
		/// <value><see cref="System.Collections.Specialized.StringDictionary"/> with the elements found in the header body</value>
		public System.Collections.Specialized.StringDictionary ContentTypeParameters {
			get {
				return this.mt.contenttype;
			}
		}
		/// <summary>
		/// Gets Date header field
		/// </summary>
		/// <value>Date header body</value>
		public System.String Date {
			get {
				System.String tmp = this.getProperty("Date");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets <see cref="System.Text.Encoding"/> found on the headers and applies to the body
		/// </summary>
		/// <value><see cref="System.Text.Encoding"/> for the body</value>
		public System.Text.Encoding Encoding {
			get {
				this.parse();
				return this.mt.enc;
			}
		}
		/// <summary>
		/// Gets From header field
		/// </summary>
		/// <value>From header body</value>
		public System.String From {
			get {
				System.String tmp = this.getProperty("From");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets Raw headers
		/// </summary>
		/// <value>From header body</value>
		public System.String RawHeaders {
			get {
				return this.message.ReadLines( this.startpoint, this.endpoint );
			}
		}
		/// <summary>
		/// Gets Message-ID header field
		/// </summary>
		/// <value>Message-ID header body</value>
		public System.String MessageID {
			get {
				System.String tmp = this.getProperty("Message-ID");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets reply address as defined by <c>rfc 2822</c>
		/// </summary>
		/// <value>Reply address</value>
		public System.String Reply {
			get {
				if ( !this.ReplyTo.Equals(System.String.Empty) )
					return this.ReplyTo;
				else
					return this.From;
			}
		}
		/// <summary>
		/// Gets Reply-To header field
		/// </summary>
		/// <value>Reply-To header body</value>
		public System.String ReplyTo {
			get {
				System.String tmp = this.getProperty("Reply-To");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets Return-Path header field
		/// </summary>
		/// <value>Return-Path header body</value>
		public System.String ReturnPath {
			get {
				System.String tmp = this.getProperty("Return-Path");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets Sender header field
		/// </summary>
		/// <value>Sender header body</value>
		public System.String Sender {
			get {
				System.String tmp = this.getProperty("Sender");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets Subject header field
		/// </summary>
		/// <value>Subject header body</value>
		public System.String Subject {
			get {
				System.String tmp = this.getProperty("Subject");
				if ( tmp==null )
					tmp = System.String.Empty;
				return tmp;
			}
		}
		/// <summary>
		/// Gets SubType from Content-Type header field
		/// </summary>
		/// <value>SubType from Content-Type header field</value>
		public System.String SubType {
			get {
				this.parse();
				return this.mt.subtype;
			}
		}
		/// <summary>
		/// Gets To header field
		/// </summary>
		/// <value>To header body</value>
		public System.String To {
			get {
				System.String tmp = this.getProperty("To");
				if ( tmp==null )
					tmp = System.String.Empty;
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
		/// <summary>
		/// Gets top-level media type from Content-Type header field
		/// </summary>
		/// <value>Top-level media type from Content-Type header field</value>
		public anmar.SharpMimeTools.MimeTopLevelMediaType TopLevelMediaType {
			get {
				this.parse();
				return this.mt.TopLevelMediaType;
			}
		}
		/// <summary>
		/// Gets Version header field
		/// </summary>
		/// <value>Version header body</value>
		public System.String Version {
			get {
				System.String tmp = this.getProperty("Version");
				if ( tmp==null )
					tmp = "1.0";
				else
					tmp = anmar.SharpMimeTools.SharpMimeTools.uncommentString ( tmp );
				return tmp;
			}
		}
	}
	/// <summary>
	/// RFC 2046 Initial top-level media types
	/// </summary>
	public enum MimeTopLevelMediaType {
		/// <summary>
		/// RFC 2046 section 4.1
		/// </summary>
		text,
		/// <summary>
		/// RFC 2046 section 4.2
		/// </summary>
		image,
		/// <summary>
		/// RFC 2046 section 4.3
		/// </summary>
		audio,
		/// <summary>
		/// RFC 2046 section 4.4
		/// </summary>
		video,
		/// <summary>
		/// RFC 2046 section 4.5
		/// </summary>
		application,
		/// <summary>
		/// RFC 2046 section 5.1
		/// </summary>
		multipart,
		/// <summary>
		/// RFC 2046 section 5.2
		/// </summary>
		message
	}
}
