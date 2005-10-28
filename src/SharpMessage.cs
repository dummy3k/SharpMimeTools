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
	/// This class provides a simplified way of parsing messages. 
	/// </summary>
	/// <remarks> All the mime complexity is handled internally and all the content is exposed
	/// parsed and decoded. The code takes care of comments, RFC 2047, encodings, etc.</remarks>
	/// <example> This sample shows how simple is to parse an e-mail message read from a file.
	/// <code>
	/// System.IO.FileStream msg = new System.IO.FileStream ( "message_file.txt", System.IO.FileMode.Open);
	/// anmar.SharpMimeTools.SharpMessage message = new anmar.SharpMimeTools.SharpMessage(msg);
	/// Console.WriteLine(message.From);
	/// Console.WriteLine(message.FromAddress);
	/// Console.WriteLine(message.To);
	/// Console.WriteLine(message.Date);
	/// Console.WriteLine(message.Subject);
	/// Console.WriteLine(message.Body);
	/// </code>
	/// </example>
	public sealed class SharpMessage {
		private static log4net.ILog log  = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private System.Collections.ArrayList _attachments;
		private System.String _body = System.String.Empty;
		private bool _body_html = false;
		private System.DateTime _date;
		private System.String _from_addr = System.String.Empty;
		private System.String _from_name = System.String.Empty;
		private anmar.SharpMimeTools.SharpMimeHeader _headers;
		private System.String _subject = System.String.Empty;
		private anmar.SharpMimeTools.SharpMimeAddressCollection _to;

		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpMessage" /> class based on the supplied <see cref="System.IO.Stream" />.
		/// </summary>
		/// <param name="message"><see cref="System.IO.Stream" /> that contains the message content</param>
		/// <remarks>The message content is automatically parsed.</remarks>
		public SharpMessage( System.IO.Stream message ) : this(message, true, true) {
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpMessage" /> class based on the supplied <see cref="System.IO.Stream" />.
		/// </summary>
		/// <param name="message"><see cref="System.IO.Stream" /> that contains the message content</param>
		/// <param name="attachments"><b>true</b> to allow attachments; <b>false</b> to skip them.</param>
		/// <param name="html"><b>true</b> to allow HTML content; <b>false</b> to ignore the html content.</param>
		/// <remarks>When the <b>attachments</b> parameter is true it's equivalent to adding <b>anmar.SharpMimeTools.MimeTopLevelMediaType.application</b>, <b>anmar.SharpMimeTools.MimeTopLevelMediaType.audio</b>, <b>anmar.SharpMimeTools.MimeTopLevelMediaType.image</b>, <b>anmar.SharpMimeTools.MimeTopLevelMediaType.video</b> to the allowed <see cref="anmar.SharpMimeTools.MimeTopLevelMediaType" />.<br />
		/// <b>anmar.SharpMimeTools.MimeTopLevelMediaType.text</b>, <b>anmar.SharpMimeTools.MimeTopLevelMediaType.multipart</b> and <b>anmar.SharpMimeTools.MimeTopLevelMediaType.message</b> are allowed in any case.<br /><br />
		/// In order to have better control over what is parsed, see the other contructors.
		/// </remarks>
		public SharpMessage( System.IO.Stream message, bool attachments, bool html ) : this(message, attachments, html, null) {
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpMessage" /> class based on the supplied <see cref="System.IO.Stream" />.
		/// </summary>
		/// <param name="message"><see cref="System.IO.Stream" /> that contains the message content</param>
		/// <param name="attachments"><b>true</b> to allow attachments; <b>false</b> to skip them.</param>
		/// <param name="html"><b>true</b> to allow HTML content; <b>false</b> to ignore the html content.</param>
		/// <param name="path">A <see cref="System.String" /> specifying the path on which to save the attachments found.</param>
		/// <remarks>When the <b>attachments</b> parameter is true it's equivalent to adding <b>anmar.SharpMimeTools.MimeTopLevelMediaType.application</b>, <b>anmar.SharpMimeTools.MimeTopLevelMediaType.audio</b>, <b>anmar.SharpMimeTools.MimeTopLevelMediaType.image</b>, <b>anmar.SharpMimeTools.MimeTopLevelMediaType.video</b> to the allowed <see cref="anmar.SharpMimeTools.MimeTopLevelMediaType" />.<br />
		/// <b>anmar.SharpMimeTools.MimeTopLevelMediaType.text</b>, <b>anmar.SharpMimeTools.MimeTopLevelMediaType.multipart</b> and <b>anmar.SharpMimeTools.MimeTopLevelMediaType.message</b> are allowed in any case.<br /><br />
		/// In order to have better control over what is parsed, see the other contructors.
		/// </remarks>
		public SharpMessage( System.IO.Stream message, bool attachments, bool html, System.String path ) {
			anmar.SharpMimeTools.MimeTopLevelMediaType types = anmar.SharpMimeTools.MimeTopLevelMediaType.text|anmar.SharpMimeTools.MimeTopLevelMediaType.multipart|anmar.SharpMimeTools.MimeTopLevelMediaType.message;
			if ( attachments )
				types = types|anmar.SharpMimeTools.MimeTopLevelMediaType.application|anmar.SharpMimeTools.MimeTopLevelMediaType.audio|anmar.SharpMimeTools.MimeTopLevelMediaType.image|anmar.SharpMimeTools.MimeTopLevelMediaType.video;
			if ( !System.IO.Directory.Exists(path) )
				this.ParseMessage(message, types, html, null);
			else
				this.ParseMessage(message, types, html, path);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpMessage" /> class based on the supplied <see cref="System.IO.Stream" />.
		/// </summary>
		/// <param name="message"><see cref="System.IO.Stream" /> that contains the message content</param>
		/// <param name="types">A <see cref="anmar.SharpMimeTools.MimeTopLevelMediaType" /> value that specifies the allowed Mime-Types to being decoded.</param>
		/// <param name="html"><b>true</b> to allow HTML content; <b>false</b> to ignore the html content.</param>
		public SharpMessage( System.IO.Stream message, anmar.SharpMimeTools.MimeTopLevelMediaType types, bool html ) {
			this.ParseMessage(message, types, html, null);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpMessage" /> class based on the supplied <see cref="System.IO.Stream" />.
		/// </summary>
		/// <param name="message"><see cref="System.IO.Stream" /> that contains the message content</param>
		/// <param name="types">A <see cref="anmar.SharpMimeTools.MimeTopLevelMediaType" /> value that specifies the allowed Mime-Types to being decoded.</param>
		/// <param name="html"><b>true</b> to allow HTML content; <b>false</b> to ignore the html content.</param>
		/// <param name="path">A <see cref="System.String" /> specifying the path on which to save the attachments found.</param>
		public SharpMessage( System.IO.Stream message, anmar.SharpMimeTools.MimeTopLevelMediaType types, bool html, System.String path ) {
			if ( !System.IO.Directory.Exists(path) )
				this.ParseMessage(message, types, html, null);
			else
				this.ParseMessage(message, types, html, path);
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpMessage" /> class based on the supplied <see cref="System.String" />.
		/// </summary>
		/// <param name="message"><see cref="System.String" /> with the message content</param>
		public SharpMessage( System.String message ) : this (new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(message))) {
		}

		/// <summary>
		/// <see cref="System.Collections.ICollection" /> that contains the attachments found in this message.
		/// </summary>
		/// <remarks>Each attachment is a <see cref="SharpAttachment" /> instance.</remarks>
		public System.Collections.ICollection Attachments {
			get { return this._attachments; }
		}
		/// <summary>
		/// Text body
		/// </summary>
		/// <remarks>If there are more than one text part in the message, they are concatenated.</remarks>
		public System.String Body {
			get { return this._body; }
		}
		/// <summary>
		/// Collection of <see cref="anmar.SharpMimeTools.SharpMimeAddress" /> instances found in the <b>Cc</b> header field.
		/// </summary>
		public System.Collections.IEnumerable Cc {
			get { return anmar.SharpMimeTools.SharpMimeAddressCollection.Parse(this._headers.Cc); }
		}
		/// <summary>
		/// Date
		/// </summary>
		/// <remarks>If there is not a <b>Date</b> field present in the headers (or it has an invalid format) then
		/// the date is extrated from the last <b>Received</b> field. If neither of them are found,
		/// <b>System.Date.MinValue</b> is returned.</remarks>
		public System.DateTime Date {
			get { return  this._date; }
		}
		/// <summary>
		/// From's name
		/// </summary>
		public System.String From {
			get { return this._from_name; }
		}
		/// <summary>
		/// From's e-mail address
		/// </summary>
		public System.String FromAddress {
			get { return this._from_addr; }
		}
		/// <summary>
		/// Gets a value indicating whether the body contains html content
		/// </summary>
		/// <value><b>true</b> if the body contains html content; otherwise, <b>false</b></value>
		public bool HasHtmlBody {
			get { return this._body_html; }
		}
		/// <summary>
		/// <see cref="anmar.SharpMimeTools.SharpMimeHeader" /> instance for this message that contains the raw content of the headers.
		/// </summary>
		public anmar.SharpMimeTools.SharpMimeHeader Headers {
			get { return this._headers; }
		}
		/// <summary>
		/// <b>Message-ID</b> header
		/// </summary>
		public System.String MessageID {
			get { return this._headers.MessageID; }
		}
		/// <summary>
		/// <b>Subject</b> field
		/// </summary>
		/// <remarks>The field body is automatically RFC 2047 decoded if it's necessary</remarks>
		public System.String Subject {
			get { return this._subject; }
		}
		/// <summary>
		/// Collection of <see cref="anmar.SharpMimeTools.SharpMimeAddress" /> found in the <b>To</b> header field.
		/// </summary>
		public System.Collections.IEnumerable To {
			get { return this._to; }
		}
		/// <summary>
		/// Returns the requested header field body.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <remarks>The value if present is uncommented and decoded (RFC 2047).<br />
		/// If the requested field is not present in this instance, <see cref="System.String.Empty" /> is returned instead.</remarks>
		public System.String GetHeaderField ( System.String name ) {
			if ( this._headers==null )
				return System.String.Empty;
			return this._headers.GetHeaderField(name, System.String.Empty, true, true);
		}

		private void ParseMessage ( System.IO.Stream stream, anmar.SharpMimeTools.MimeTopLevelMediaType types, bool html, System.String path ) {
			this._attachments = new System.Collections.ArrayList();
			anmar.SharpMimeTools.SharpMimeMessage message = new anmar.SharpMimeTools.SharpMimeMessage(stream);
			this.ParseMessage(message, types, html, path);
			this._headers = message.Header;
			message.Close();
			message = null;
			// Date
			this._date = anmar.SharpMimeTools.SharpMimeTools.parseDate(this._headers.Date);
			if ( this._date.Equals(System.DateTime.MinValue) ) {
				System.String date = this._headers["Received"];
				if ( date==null )
					date = System.String.Empty;
				if ( date.IndexOf("\r\n")>0 )
					date = date.Substring(0, date.IndexOf("\r\n"));
				if ( date.LastIndexOf(';')>0 )
					date = date.Substring(date.LastIndexOf(';')+1).Trim();
				else
					date = System.String.Empty;
				this._date = anmar.SharpMimeTools.SharpMimeTools.parseDate(date);
			}
			// Subject
			this._subject = anmar.SharpMimeTools.SharpMimeTools.parserfc2047Header(this._headers.Subject);
			// To
			this._to = anmar.SharpMimeTools.SharpMimeAddressCollection.Parse(this._headers.To);
			// From
			anmar.SharpMimeTools.SharpMimeAddressCollection from = anmar.SharpMimeTools.SharpMimeAddressCollection.Parse(this._headers.From);
			foreach ( anmar.SharpMimeTools.SharpMimeAddress item in from ) {
				this._from_name = item["name"];
				this._from_addr = item["address"];
				if ( this._from_name==null || this._from_name.Equals(System.String.Empty) )
					this._from_name = item["address"];
			}
		}
		private void ParseMessage ( anmar.SharpMimeTools.SharpMimeMessage part, anmar.SharpMimeTools.MimeTopLevelMediaType types, bool html, System.String path ) {
			if ( !(types&part.Header.TopLevelMediaType).Equals(part.Header.TopLevelMediaType) ) {
				if ( log.IsDebugEnabled )
					log.Debug (System.String.Concat("Mime-Type [", part.Header.TopLevelMediaType, "] is not an accepted Mime-Type. Skiping part."));
				return;
			}
			switch ( part.Header.TopLevelMediaType ) {
				case anmar.SharpMimeTools.MimeTopLevelMediaType.multipart:
				case anmar.SharpMimeTools.MimeTopLevelMediaType.message:
					// TODO: allow other subtypes of "message"
					// Only message/rfc822 allowed, other subtypes ignored
					if ( part.Header.TopLevelMediaType.Equals(anmar.SharpMimeTools.MimeTopLevelMediaType.message)
						 && !part.Header.SubType.Equals("rfc822") )
						break;
					if ( part.Header.SubType.Equals ("alternative") ) {
						if ( part.PartsCount>0 ) {
							// Get the first mime part of the alternatives that has a accepted Mime-Type
							for ( int i=part.PartsCount; i>0; i-- ) {
								anmar.SharpMimeTools.SharpMimeMessage item = part.GetPart(i-1);
								if ( !(types&item.Header.TopLevelMediaType).Equals(item.Header.TopLevelMediaType) 
								    || ( !html && item.Header.TopLevelMediaType.Equals(anmar.SharpMimeTools.MimeTopLevelMediaType.text) && item.Header.SubType.Equals("html") )
								   ) {
									if ( log.IsDebugEnabled )
										log.Debug (System.String.Concat("Mime-Type [", item.Header.TopLevelMediaType, "/", item.Header.SubType, "] is not an accepted Mime-Type. Skiping alternative part."));
									continue;
								}
								this.ParseMessage(item, types, html, path);
								break;
							}
						}
					// TODO: Take into account each subtype of "multipart"
					} else if ( part.PartsCount>0 ) {
						foreach ( anmar.SharpMimeTools.SharpMimeMessage item in part ) {
							this.ParseMessage(item, types, html, path);
						}
					}
					break;
				case anmar.SharpMimeTools.MimeTopLevelMediaType.text:
					if ( ( part.Disposition==null || !part.Disposition.Equals("attachment") )
						&& ( part.Header.SubType.Equals("plain") || part.Header.SubType.Equals("html") ) ) {
						// HTML content not allowed
						if ( part.Header.SubType.Equals("html") ) {
							if ( !html )
								break;
							else
								this._body_html=true;
							
						}
						if ( this._body_html && part.Header.SubType.Equals("plain") ) {
							this._body = System.String.Concat (this._body, "<pre>", part.BodyDecoded, "</pre>");
						} else 
							this._body = System.String.Concat (this._body, part.BodyDecoded);
						break;
					} else {
						goto case anmar.SharpMimeTools.MimeTopLevelMediaType.application;
					}
				case anmar.SharpMimeTools.MimeTopLevelMediaType.application:
				case anmar.SharpMimeTools.MimeTopLevelMediaType.audio:
				case anmar.SharpMimeTools.MimeTopLevelMediaType.image:
				case anmar.SharpMimeTools.MimeTopLevelMediaType.video:
					anmar.SharpMimeTools.SharpAttachment attachment = null;
					// Save to a file
					if ( path!=null ) {
						System.IO.FileInfo file = part.DumpBody(path, true);
						if ( file!=null ) {
							attachment = new anmar.SharpMimeTools.SharpAttachment(file);
							attachment.Name = file.Name;
						}
					// Save to a stream
					} else {
						System.IO.MemoryStream stream = new System.IO.MemoryStream();
						if ( part.DumpBody(stream) ) {
							attachment = new anmar.SharpMimeTools.SharpAttachment(stream);
							if ( part.Name!=null )
								attachment.Name = part.Name;
							else
								attachment.Name = System.String.Concat("generated_", part.GetHashCode(), ".", part.Header.SubType);
						}
						stream = null;
					}
					if ( attachment!=null )
						this._attachments.Add(attachment);
					break;
				default:
					break;
			}
		}
	}
	/// <summary>
	/// This class provides the basic functionality for handling attachments
	/// </summary>
	public class SharpAttachment {
		private static log4net.ILog log  = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private System.String _name;
		private System.IO.MemoryStream _stream;
		private System.IO.FileInfo _saved_file;

		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpAttachment" /> class based on the supplied <see cref="System.IO.MemoryStream" />.
		/// </summary>
		/// <param name="stream"><see cref="System.IO.MemoryStream" /> instance that contains the attachment content.</param>
		public SharpAttachment ( System.IO.MemoryStream stream ) {
			this._stream = stream;
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="anmar.SharpMimeTools.SharpAttachment" /> class based on the supplied <see cref="System.IO.FileInfo" />.
		/// </summary>
		/// <param name="file"><see cref="System.IO.MemoryStream" /> instance that contains the info about the attachment.</param>
		public SharpAttachment ( System.IO.FileInfo file ) {
			this._saved_file = file;
		}
		/// <summary>
		/// Closes the underling stream if it's open.
		/// </summary>
		public void Close() {
			if ( this._stream!=null && this._stream.CanRead )
				this._stream.Close();
			this._stream = null;
		}
		/// <summary>
		/// Saves of the attachment to a file in the given path.
		/// </summary>
		/// <param name="path">A <see cref="System.String" /> specifying the path on which to save the attachment.</param>
		/// <param name="overwrite"><b>true</b> if the destination file can be overwritten; otherwise, <b>false</b>.</param>
		/// <returns><see cref="System.IO.FileInfo" /> of the saved file. <b>null</b> when it fails to save.</returns>
		/// <remarks>If the file was already saved, the previous <see cref="System.IO.FileInfo" /> is returned.<br />
		/// Once the file is successfully saved, the stream is closed and <see cref="SavedFile" /> property is updated.</remarks>
		public System.IO.FileInfo Save ( System.String path, bool overwrite ) {
			if ( path==null || this._name==null )
				return null;
			if ( this._stream==null ) {
				if ( this._saved_file!=null )
					return this._saved_file;
				else
					return null;
			}
			if ( !this._stream.CanRead ) {
				if ( log.IsErrorEnabled )
					log.Error(System.String.Concat("The provided stream does not support reading."));
				return null;
			}
			System.IO.FileInfo file = new System.IO.FileInfo (System.IO.Path.Combine (path, this._name));
			if ( !file.Directory.Exists ) {
				if ( log.IsErrorEnabled )
					log.Error(System.String.Concat("Destination folder [", file.Directory.FullName, "] does not exist"));
				return null;
			}
			if ( file.Exists ) {
				if ( overwrite ) {
					try {
						file.Delete();
					} catch ( System.Exception e ) {
						if ( log.IsErrorEnabled )
							log.Error(System.String.Concat("Error deleting existing file[", file.FullName, "]"), e);
						return null;
					}
				} else {
					if ( log.IsErrorEnabled )
						log.Error(System.String.Concat("Destination file [", file.FullName, "] already exists"));
					return null;
				}
			}
			try {
				System.IO.FileStream stream = file.OpenWrite();
				this._stream.WriteTo(stream);
				stream.Flush();
				stream.Close();
				this.Close();
				this._saved_file = file;
			} catch ( System.Exception e ) {
				if ( log.IsErrorEnabled )
						log.Error(System.String.Concat("Error writting file [", file.FullName, "]"), e);
					return null;
			}
			return file;
		}
		/// <summary>
		/// Gets or sets the name of the attachment.
		/// </summary>
		/// <value>The name of the attachment.</value>
		public System.String Name {
			get { return this._name; }
			set {
				if ( value!=null )
					this._name = System.IO.Path.GetFileName(value);
				else
					this._name = null;
			}
		}
		/// <summary>
		/// Gets the <see cref="System.IO.FileInfo" /> of the saved file.
		/// </summary>
		/// <value>The <see cref="System.IO.FileInfo" /> of the saved file.</value>
		public System.IO.FileInfo SavedFile {
			get { return this._saved_file; }
		}
		/// <summary>
		/// Gets the <see cref="System.IO.Stream " /> of the attachment.
		/// </summary>
		/// <value>The <see cref="System.IO.Stream " /> of the attachment.</value>
		/// <remarks>If the underling stream exists, it's returned. If the file has been saved, it opens <see cref="SavedFile" /> for reading.</remarks>
		public System.IO.Stream Stream {
			get {
				if ( this._stream!=null )
					return this._stream;
				else if ( this._saved_file!=null )
					return this._saved_file.OpenRead();
				else
					return null;
			}
		}
	}
}
