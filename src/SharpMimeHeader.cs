using System;

namespace anmar.SharpMimeTools {
	public class SharpMimeHeader : System.Collections.IEnumerable {
		private static log4net.ILog log  = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		internal anmar.SharpMimeTools.SharpMimeMessageStream message;
		protected System.Collections.Specialized.HybridDictionary headers;
		protected long startpoint;
		protected long endpoint;
		protected long startbody;
		protected System.Text.Encoding enc = new System.Text.ASCIIEncoding();

		protected struct HeaderInfo {
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
		protected HeaderInfo mt;

		internal SharpMimeHeader(anmar.SharpMimeTools.SharpMimeMessageStream message) {
			this.startpoint = 0;
			this.message = message;
			this.headers = new System.Collections.Specialized.HybridDictionary(2, true);
			this.parse();
		}
		internal SharpMimeHeader(anmar.SharpMimeTools.SharpMimeMessageStream message, long startpoint) {
			this.startpoint = startpoint;
			this.message = message;
			this.headers = new System.Collections.Specialized.HybridDictionary(2, true);
			this.parse();
		}
		public SharpMimeHeader( System.IO.Stream message ) {
			this.startpoint = 0;
			this.message = new anmar.SharpMimeTools.SharpMimeMessageStream (message);
			this.headers = new System.Collections.Specialized.HybridDictionary(2, true);
			this.parse();
		}
		public SharpMimeHeader( System.IO.Stream message, long startpoint ) {
			this.startpoint = startpoint;
			this.message = new anmar.SharpMimeTools.SharpMimeMessageStream (message);
			this.headers = new System.Collections.Specialized.HybridDictionary(2, true);
			this.parse();
		}
		public System.String this[ System.Object name ] {
			get {
				return this.getProperty( name.ToString() );
			}
		}
		public System.Collections.IEnumerator GetEnumerator() {
			return headers.GetEnumerator();
		}
		protected System.String getProperty (  System.String name ) {
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
		public long BodyPosition {
			get {
				return this.startbody;
			}
		}
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
		public int Count {
			get {
				return this.headers.Count;
			}
		}
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
		public System.Collections.Specialized.StringDictionary ContentDispositionParameters {
			get {
				return this.mt.contentdisposition;
			}
		}
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
		public System.Collections.Specialized.StringDictionary ContentLocationParameters {
			get {
				return this.mt.contentlocation;
			}
		}
		public System.String ContentType {
			get {
				System.String tmp = this.getProperty("Content-Type");
				if ( tmp==null ) {
					tmp = "text/plain; charset=us-ascii";
				}
				return tmp;
			}
		}
		public System.Collections.Specialized.StringDictionary ContentTypeParameters {
			get {
				return this.mt.contenttype;
			}
		}
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
		public System.Text.Encoding Encoding {
			get {
				this.parse();
				return this.mt.enc;
			}
		}
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
		public System.String RawHeaders {
			get {
				return this.message.ReadLines( this.startpoint, this.endpoint );
			}
		}
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
		public System.String Reply {
			get {
				if ( !this.ReplyTo.Equals(System.String.Empty) )
					return this.ReplyTo;
				else
					return this.From;
			}
		}
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
		public System.String Subject {
			get {
				System.String tmp = this.getProperty("Subject");
				if ( tmp==null )
					tmp = System.String.Empty;
				return tmp;
			}
		}
		public System.String SubType {
			get {
				this.parse();
				return this.mt.subtype;
			}
		}
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
		public anmar.SharpMimeTools.MimeTopLevelMediaType TopLevelMediaType {
			get {
				this.parse();
				return this.mt.TopLevelMediaType;
			}
		}
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
	// RFC 2046 Initial Top-Level Media Types
	public enum MimeTopLevelMediaType {
		text,
		image,
		audio,
		video,
		application,
		multipart,
		message
	}
}
