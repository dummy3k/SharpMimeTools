using System;

namespace anmar.SharpMimeTools
{
	internal class SharpMimeAddressCollection : System.Collections.IEnumerable {
		// Create a logger for use in this class
		private static log4net.ILog log  = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		protected System.Collections.ArrayList list = new System.Collections.ArrayList();

		public SharpMimeAddressCollection ( System.String text ) {
			string[] tokens = text.Split( new char[] { ',' , ';' } );
			foreach ( System.String token in tokens ) {
				this.Add ( new anmar.SharpMimeTools.SharpMimeAddress( token ));
			}
		}
		public anmar.SharpMimeTools.SharpMimeAddress this [ int index ] {
			get {
					return this.Get( index );
			}
		}
		public System.Collections.IEnumerator GetEnumerator() {
			return list.GetEnumerator();
		}
		public void Add ( anmar.SharpMimeTools.SharpMimeAddress address ) {
			list.Add ( address);
		}
		public anmar.SharpMimeTools.SharpMimeAddress Get ( int index ) {
			return (anmar.SharpMimeTools.SharpMimeAddress) list[index];
		}
		public static anmar.SharpMimeTools.SharpMimeAddressCollection Parse( System.String text ) {
			if ( text == null )
				throw new ArgumentNullException();
			return new anmar.SharpMimeTools.SharpMimeAddressCollection ( text );
		}
		public int Count {
			get {
				return list.Count;
			}
		}
		public override string ToString() {
			System.Text.StringBuilder text = new System.Text.StringBuilder();
			foreach ( anmar.SharpMimeTools.SharpMimeAddress token in list ) {
				text.Append ( token.ToString() );
				if ( token.Length>0 )
					text.Append ("; ");
			}
			return text.ToString(); 
		}
	}
	public class SharpMimeAddress {
		protected System.String name;
		protected System.String address;
		public SharpMimeAddress ( System.String dir ) {
			name = anmar.SharpMimeTools.SharpMimeTools.parseFrom ( dir, 1 );
			address = anmar.SharpMimeTools.SharpMimeTools.parseFrom ( dir, 2 );
		}
		public System.String this [object key] {
			get {
				if ( key == null ) throw new ArgumentNullException();
				switch (key.ToString()) {
					case "0":
					case "name":
						return this.name;
					case "1":
					case "address":
						return this.address;
				}
				return null;
			}
		}
		public override string ToString() {
			if ( this.name.Equals (System.String.Empty ) && this.address.Equals (System.String.Empty ) )
				return "";
			if ( this.name.Equals (System.String.Empty ) )
				return String.Format( "<{0}>", this.address);
			else
				return String.Format( "\"{0}\" <{1}>" , this.name , this.address);
		}
		public int Length {
			get {
				return this.name.Length + this.address.Length;
			}
		}
	}
}
