using System;

namespace anmar.SharpMimeTools {

	internal class SharpMimeMessageCollection : System.Collections.IEnumerable {
		private static log4net.ILog log  = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		protected anmar.SharpMimeTools.SharpMimeMessage parent;
		protected System.Collections.ArrayList messages = new System.Collections.ArrayList();
	
		public SharpMimeMessage this[ int index ] {
			get { return this.Get( index ); }
		}
		public void Add ( anmar.SharpMimeTools.SharpMimeMessage msg ) {
			messages.Add( msg );
		}
		public anmar.SharpMimeTools.SharpMimeMessage Get( int index ) {
			return (anmar.SharpMimeTools.SharpMimeMessage)messages[index];
		}
		public System.Collections.IEnumerator GetEnumerator() {
			return messages.GetEnumerator();
		}
		public int Count {
			get {
				return messages.Count;
			}
		}
		public anmar.SharpMimeTools.SharpMimeMessage Parent {
			get {
				return this.parent;
			}
			set {
				this.parent = value;
			}
		}
	}
}
