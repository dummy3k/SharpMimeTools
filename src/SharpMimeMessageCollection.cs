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
	internal class SharpMimeMessageCollection : System.Collections.IEnumerable {
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
