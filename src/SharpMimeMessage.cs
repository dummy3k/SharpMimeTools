// -----------------------------------------------------------------------
//
//   Copyright (C) 2003-2005 Angel Marin
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
	/// rfc 2045 entity
	/// </summary>
	public class SharpMimeMessage : System.Collections.IEnumerable {
		private struct MessageInfo {
			internal long start;
			internal long start_body;
			internal long end;
			internal anmar.SharpMimeTools.SharpMimeHeader header;
			internal anmar.SharpMimeTools.SharpMimeMessageCollection parts;

			internal MessageInfo ( anmar.SharpMimeTools.SharpMimeMessageStream m, long start ) {
				this.start = start;
				this.header = new anmar.SharpMimeTools.SharpMimeHeader ( m, this.start );
				this.start_body = this.header.BodyPosition;
				this.end = -1;
				parts = new anmar.SharpMimeTools.SharpMimeMessageCollection();
			}
		}
		private static log4net.ILog log  = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private anmar.SharpMimeTools.SharpMimeMessageStream message;
		private MessageInfo mi;

		/// <summary>
		/// Initializes a new instance of the <see cref="SharpMimeMessage"/> class from a <see cref="System.IO.Stream"/>
		/// </summary>
		/// <param name="message"><see cref="System.IO.Stream" /> to read the message from</param>
		public SharpMimeMessage( System.IO.Stream message ) {
			this.message = new anmar.SharpMimeTools.SharpMimeMessageStream (message);
			this.mi = new MessageInfo ( this.message, this.message.Stream.Position );
		}
		private SharpMimeMessage( anmar.SharpMimeTools.SharpMimeMessageStream message, long startpoint ) {
			this.message = message;
			this.mi = new MessageInfo ( this.message, startpoint );
		}
		private SharpMimeMessage( anmar.SharpMimeTools.SharpMimeMessageStream message, long startpoint, long endpoint ) {
			this.message = message;
			this.mi = new MessageInfo ( this.message, startpoint );
			this.mi.end = endpoint;
		}
		/// <summary>
		/// Clears the parts references contained in this instance and calls the <b>Close</b> method in those parts.
		/// </summary>
		/// <remarks>This method does not close the underling <see cref="System.IO.Stream" /> used to create this instance.</remarks>
		public void Close() {
			foreach ( anmar.SharpMimeTools.SharpMimeMessage part in this.mi.parts )
				part.Close();
			this.mi.parts.Clear();
		}
		/// <summary>
		/// Dumps the body of this entity into a <see cref="System.IO.Stream"/>
		/// </summary>
		/// <param name="stream"><see cref="System.IO.Stream" /> where we want to write the body</param>
		/// <returns><b>true</b> OK;<b>false</b> if write operation fails</returns>
		public bool DumpBody ( System.IO.Stream stream ) {
			bool error = false;
			if ( stream.CanWrite ) {
				System.Byte[] buffer = null;
				switch (this.Header.ContentTransferEncoding) {
					case "quoted-printable":
						buffer = this.mi.header.Encoding.GetBytes(this.BodyDecoded);
						break;
					case "base64":
						try {
							buffer = System.Convert.FromBase64String(this.Body);
						} catch ( System.Exception e ) {
							error = true;
							if ( log.IsErrorEnabled )
								log.Error("Error Converting base64 string", e);
						}
						break;
					case "7bit":
					case "8bit":
					case "binary":
					case null:
						buffer = System.Text.Encoding.ASCII.GetBytes(this.Body);
						break;
					default:
						error=true;
						break;
				}
				try {
					if ( !error && buffer!=null )
						stream.Write ( buffer, 0, buffer.Length );
				} catch ( System.Exception e ) {
					error = true;
					if ( log.IsErrorEnabled )
						log.Error("Error dumping body", e);
				}
				buffer = null;
			} else {
				error = true;
			}
			return !error;
		}
		/// <summary>
		/// Dumps the body of this entity into a file
		/// </summary>
		/// <param name="path">path of the destination folder</param>
		/// <returns><see cref="System.IO.FileInfo" /> that represents the file where the body has been saved</returns>
		public System.IO.FileInfo DumpBody ( System.String path ) {
			return this.DumpBody ( path, this.Name );
		}
		/// <summary>
		/// Dumps the body of this entity into a file
		/// </summary>
		/// <param name="path">path of the destination folder</param>
		/// <param name="generatename">true if the filename must be generated incase we can't find a valid one in the headers</param>
		/// <returns><see cref="System.IO.FileInfo" /> that represents the file where the body has been saved</returns>
		public System.IO.FileInfo DumpBody ( System.String path, bool generatename ) {
			System.String name = this.Name;
			if ( name==null && generatename )
				name = System.String.Format ( "generated_{0}.{1}", this.GetHashCode(), this.Header.SubType );
			return this.DumpBody ( path, name );
		}
		/// <summary>
		/// Dumps the body of this entity into a file
		/// </summary>
		/// <param name="path">path of the destination folder</param>
		/// <param name="name">name of the file</param>
		/// <returns><see cref="System.IO.FileInfo" /> that represents the file where the body has been saved</returns>
		public System.IO.FileInfo DumpBody ( System.String path, System.String name ) {
			System.IO.FileInfo file = null;
			if ( name!=null ) {
				if ( log.IsDebugEnabled )
					log.Debug ("Found attachment: " + name);
				name = System.IO.Path.GetFileName(name);
				// Dump file contents
				try {
					System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo ( path );
					dir.Create();
					file = new System.IO.FileInfo (System.IO.Path.Combine (path, name) );
					if ( dir.Exists
						 && dir.FullName.Equals (new System.IO.DirectoryInfo (file.Directory.FullName).FullName) ) {
						if ( !file.Exists ) {
							if ( this.Header.ContentDispositionParameters.ContainsKey("creation-date") )
								file.CreationTime = anmar.SharpMimeTools.SharpMimeTools.parseDate ( this.Header.ContentDispositionParameters["creation-date"] );
							if ( this.Header.ContentDispositionParameters.ContainsKey("modification-date") )
								file.LastWriteTime = anmar.SharpMimeTools.SharpMimeTools.parseDate ( this.Header.ContentDispositionParameters["modification-date"] );
							if ( this.Header.ContentDispositionParameters.ContainsKey("read-date") )
								file.LastAccessTime = anmar.SharpMimeTools.SharpMimeTools.parseDate ( this.Header.ContentDispositionParameters["read-date"] );
							System.IO.Stream stream = file.Create();
							bool error = !this.DumpBody (stream);
							stream.Close();
							if ( error ) {
								if ( log.IsErrorEnabled )
									log.Error ("Error writtin to disk: " + name);
								file.Delete();
							} else {
								if ( log.IsDebugEnabled )
									log.Debug ("Attachment saved: " + name);
								// The file should be there
								file.Refresh();
							}
						}
					}
					dir = null;
				} catch ( System.Exception e ) {
					if ( log.IsErrorEnabled )
						log.Error ("Error writting to disk: " + name, e);
					try {
						if ( file!=null ) {
							file.Refresh();
							if ( file.Exists )
								file.Delete ();
						}
					} catch ( System.Exception ) {}
					file = null;
				}
			}
			return file;
		}
		/// <summary>
		/// Returns an enumerator that can iterate through the parts of a multipart entity
		/// </summary>
		/// <returns>A <see cref="System.Collections.IEnumerator" /> for the parts of a multipart entity</returns>
		public System.Collections.IEnumerator GetEnumerator() {
			this.parse();
			return this.mi.parts.GetEnumerator();
		}
		/// <summary>
		/// Returns the requested part of a multipart entity
		/// </summary>
		/// <param name="index">index of the requested part</param>
		/// <returns>A <see cref="anmar.SharpMimeTools.SharpMimeMessage" /> for the requested part</returns>
		public anmar.SharpMimeTools.SharpMimeMessage GetPart ( int index ) {
			return this.Parts.Get ( index );
		}
		private bool parse () {
			bool error = false;
			if ( log.IsDebugEnabled ) log.Debug (System.String.Concat("Parsing requested, type: ", this.mi.header.TopLevelMediaType.ToString(), ", subtype: ", this.mi.header.SubType) );
			if ( !this.IsMultipart || this.Equals(this.mi.parts.Parent) ) {
				if ( log.IsDebugEnabled )
					log.Debug ("Parsing requested and this is not a multipart or it is already parsed");
				return true;
			}
			switch (this.mi.header.TopLevelMediaType) {
				case anmar.SharpMimeTools.MimeTopLevelMediaType.message:
					this.mi.parts.Parent = this;
					anmar.SharpMimeTools.SharpMimeMessage message = new anmar.SharpMimeTools.SharpMimeMessage (this.message, this.mi.start_body, this.mi.end );
					this.mi.parts.Add (message);
					break;
				case anmar.SharpMimeTools.MimeTopLevelMediaType.multipart:
					this.message.SeekPoint ( this.mi.start_body );
					System.String line;
					if ( log.IsDebugEnabled )
						log.Debug (System.String.Format("Looking for multipart {1}, byte {0}", this.mi.start_body, this.mi.header.ContentTypeParameters["boundary"]));
					this.mi.parts.Parent = this;
					for ( line=this.message.ReadLine(); line!=null ; line=this.message.ReadLine() ) {
						if ( line.Equals( "--" + this.mi.header.ContentTypeParameters["boundary"] ) ) {
							if ( this.mi.parts.Count>0 ) {
								this.mi.parts.Get( this.mi.parts.Count-1 ).mi.end = this.message.Position_preRead;
								if ( log.IsDebugEnabled )
									log.Debug (System.String.Format("End part {1} at byte {0}", this.message.Position_preRead, this.mi.header.ContentTypeParameters["boundary"]));
							}
							if ( log.IsDebugEnabled ) log.Debug (System.String.Format("Part     {1} found at byte {0}", this.message.Position_preRead, this.mi.header.ContentTypeParameters["boundary"]));
							anmar.SharpMimeTools.SharpMimeMessage msg = new anmar.SharpMimeTools.SharpMimeMessage (this.message, this.message.Position );
							this.mi.parts.Add (msg);
						} else if ( line.Equals( "--" + this.mi.header.ContentTypeParameters["boundary"] + "--" ) ) {
							this.mi.end = this.message.Position_preRead;
							if ( this.mi.parts.Count>0 ) {
								this.mi.parts.Get( this.mi.parts.Count-1 ).mi.end = this.message.Position_preRead;
								if ( log.IsDebugEnabled )
									log.Debug (System.String.Format("End part {1} at byte {0}", this.message.Position_preRead, this.mi.header.ContentTypeParameters["boundary"]));
							} else if ( log.IsDebugEnabled )
								log.Debug (System.String.Format("End part {1} at byte {0}", this.mi.end, this.mi.header.ContentTypeParameters["boundary"]));
							break;
						}
					}
					break;
			}
			return !error;
		}
		/// <summary>
		/// Gets header fields for this entity
		/// </summary>
		/// <param name="name">field name</param>
		/// <remarks>Field names is case insentitive</remarks>
		public System.String this[ System.Object name ] {
			get { return this.mi.header[ name.ToString()]; }
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public System.String Body {
			get {
				this.parse();
				if ( this.mi.parts.Count == 0 ) {
					this.message.Enconding = this.mi.header.Encoding;
					if ( this.mi.end ==-1 ) {
						return this.message.ReadAll(this.mi.start_body);
					} else {
						return this.message.ReadLines(this.mi.start_body, this.mi.end);
					}
				} else {
					return null;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public System.String BodyDecoded {
			get {
				switch (this.Header.ContentTransferEncoding) {
					case "quoted-printable":
						System.String body = this.Body;
						anmar.SharpMimeTools.SharpMimeTools.QuotedPrintable2Unicode ( this.mi.header.Encoding, ref body );
						return body;
					case "base64":
						System.Byte[] tmp = null;
						try {
							tmp = System.Convert.FromBase64String(this.Body);
						} catch ( System.Exception e ) {
							if ( log.IsErrorEnabled )
								log.Error("Error dumping body", e);
						}
						if ( tmp!=null )
							return this.mi.header.Encoding.GetString(tmp);
						else
							return System.String.Empty;
				}
				return this.Body;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public System.String Disposition {
			get {
				return this.Header.ContentDispositionParameters["Content-Disposition"];
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public anmar.SharpMimeTools.SharpMimeHeader Header {
			get {
				return this.mi.header;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool IsBrowserDisplay {
			get {
				switch (this.mi.header.TopLevelMediaType) {
					case anmar.SharpMimeTools.MimeTopLevelMediaType.audio:
					case anmar.SharpMimeTools.MimeTopLevelMediaType.image:
					case anmar.SharpMimeTools.MimeTopLevelMediaType.text:
					case anmar.SharpMimeTools.MimeTopLevelMediaType.video:
						return true;
					default:
						return false;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool IsMultipart {
			get {
				switch (this.mi.header.TopLevelMediaType) {
					case anmar.SharpMimeTools.MimeTopLevelMediaType.multipart:
					case anmar.SharpMimeTools.MimeTopLevelMediaType.message:
						return true;
					default:
						return false;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool IsTextBrowserDisplay {
			get {
				if ( this.mi.header.TopLevelMediaType.Equals(anmar.SharpMimeTools.MimeTopLevelMediaType.text) && this.mi.header.SubType.Equals("plain") ) {
					return true;
				} else {
					return false;
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public System.String Name {
			get {
				this.parse();
				System.String param = this.Header.ContentDispositionParameters["filename"];

				if ( param==null ) {
					param = this.Header.ContentTypeParameters["name"];
				}
				if ( param==null ) {
					param = this.Header.ContentLocationParameters["Content-Location"];
				}
				if ( param!=null ) {
					param = param.Replace("\t", "");
					try {
						param = System.IO.Path.GetFileName(param);
					} catch ( System.ArgumentException ) {
						// Remove invalid chars
						foreach ( char ichar in System.IO.Path.InvalidPathChars ) {
							param = param.Replace ( ichar.ToString(), System.String.Empty );
						}
						param = System.IO.Path.GetFileName(param);
					}
				}
				return param;
			}
		}
		internal anmar.SharpMimeTools.SharpMimeMessageCollection  Parts {
			get {
				this.parse();
				return this.mi.parts;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public int PartsCount {
			get {
				this.parse();
				return this.mi.parts.Count;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public long Size {
			get {
				this.parse();
				return this.mi.end - this.mi.start_body;
			}
		}
	}
}
