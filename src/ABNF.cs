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
	/// 
	/// </summary>
	public sealed class ABNF {
		/// <summary>
		/// 
		/// </summary>
		public const string CRLF = "\r\n";
		/// <summary>
		/// 
		/// </summary>
		public const string ALPHA = @"A-Za-z";
		/// <summary>
		/// 
		/// </summary>
		public const string DIGIT = @"0-9";
		/// <summary>
		/// RFC 2822 Section 2.2.2
		/// </summary>
		public const string WSP = @"\x20\x09";
		/// <summary>
		/// RFC 2822 Section 3.2.1
		/// </summary>
		public const string NO_WS_CTL = @"\x01-\x08\x0B\x0C\x0E-\x1F\x7F";
		/// <summary>
		/// RFC 2822 Section 3.2.1
		/// </summary>
		// FIXME: add obs-text
		public const string text = @"\x01-\x09\x0B\x0C\x0E-\x7F";
		/// <summary>
		/// RFC 2822 Section 3.2.2
		/// </summary>
		// FIXME: add obs-qp
		public const string quoted_pair = @"\x5C[" + text + "]";
		/// <summary>
		/// RFC 2822 Section 3.2.3
		/// </summary>
		// FIXME: add obs-FWS
		public const string FWS = @"(?:(?:[" + WSP + @"]{0,}\r\n){0,1}[" + WSP + @"]+)";
		/// <summary>
		/// RFC 2822 Section 3.2.3
		/// </summary>
		// FIXME: Correct this simplification
		public const string CFWS = FWS;
		/// <summary>
		/// RFC 2822 Section 3.2.4
		/// </summary>
		public const string atext = ALPHA + DIGIT + @"\x21\x23-\x27\x2A\x2B\x2D\x2F\x3D\x3F\x5E\x5F\x60\x7B-\x7E";
		/// <summary>
		/// RFC 2822 Section 3.2.4
		/// </summary>
		public const string atom = CFWS + @"{0,1}[" + atext + @"]+" + CFWS + @"{0,1}";
		/// <summary>
		/// RFC 2822 Section 3.2.4
		/// </summary>
		public const string dot_atom = CFWS + @"{0,1}" + dot_atom_text + CFWS + @"{0,1}";
		/// <summary>
		/// RFC 2822 Section 3.2.4
		/// </summary>
		public const string dot_atom_text = @"[" + atext + @"]{1,}(?:\.[" + atext + @"]{1,}){0,}";
		/// <summary>
		/// RFC 2822 Section 3.2.5
		/// </summary>
		public const string DQUOTE = @"\x22";
		/// <summary>
		/// RFC 2822 Section 3.2.5
		/// </summary>
		public const string qtext = NO_WS_CTL + @"\x21\x23-\x5A\x5B\x5D-\x7E";
		/// <summary>
		/// RFC 2822 Section 3.2.5
		/// </summary>
		public const string qcontent =  @"(?:[" + qtext + @"]|(?:" + quoted_pair + @"))";
		/// <summary>
		/// RFC 2822 Section 3.2.5
		/// </summary>
		public const string quoted_string = CFWS + @"{0,1}" + DQUOTE + @"(?:" + FWS + @"{0,1}" + qcontent + "){0,}" + FWS + @"{0,1}" + DQUOTE + CFWS + @"{0,1}";
		/// <summary>
		/// RFC 2822 Section 3.2.6
		/// </summary>
		public const string word = @"(?:" + atom + @"|(?:" + quoted_string + @"))";
		/// <summary>
		/// RFC 2822 Section 3.2.6
		/// </summary>
		// FIXME: add obs-phrase
		public const string phrase = @"(?:" + word +  @"){1,}";
		/// <summary>
		/// RFC 2822 Section 3.2.6
		/// </summary>
		public const string address = @"(?:" + mailbox + @"|" + group + @")";
		/// <summary>
		/// RFC 2822 Section 3.2.6
		/// </summary>
		public const string mailbox = @"(?:" + addr_spec + @"|" + name_addr + @")";
		/// <summary>
		/// RFC 2822 Section 3.2.6
		/// </summary>
		public const string name_addr = @"(?:" + phrase + @"){0,}" + angle_addr;
		/// <summary>
		/// RFC 2822 Section 3.4
		/// </summary>
		// FIXME: add obs-angle-addr
		public const string angle_addr = CFWS + @"{0,1}" + @"\x3C" + addr_spec + @"\x3E" + CFWS + @"{0,1}";
		/// <summary>
		/// RFC 2822 Section 3.4
		/// </summary>
		public const string group = phrase + @":(?:" + mailbox_list + @"|" + CFWS + @"{0,1}){0,};" + CFWS + @"{0,1}";
		/// <summary>
		/// RFC 2822 Section 3.4
		/// </summary>
		public const string mailbox_list = @"(?:" + mailbox + @"(?:," + mailbox + @"){0,})";
		/// <summary>
		/// RFC 2822 Section 3.4
		/// </summary>
		public const string address_list = @"(?:" + address + @"(?:," + address + @"){0,})";
		/// <summary>
		/// RFC 2822 Section 3.4.1
		/// </summary>
		// FIXME: add obs-local-part
		public const string local_part = @"(?:" + dot_atom + @"|" + quoted_string + @")";
		/// <summary>
		/// RFC 2822 Section 3.4.1
		/// </summary>
		// FIXME: add obs-domain
		public const string domain = @"(?:" + dot_atom + @"|" + domain_literal + @")";
		/// <summary>
		/// RFC 2822 Section 3.4.1
		/// </summary>
		public const string domain_literal = CFWS + @"{0,1}" + @"\[(?:" + FWS + @"{0,1}" + dcontent + @"){0,}" + FWS + @"{0,1}" + @"\]" + CFWS + @"{0,1}";
		/// <summary>
		/// RFC 2822 Section 3.4.1
		/// </summary>
		public const string dtext = NO_WS_CTL + @"\x21-\x5A\x5E-\x7E";
		/// <summary>
		/// RFC 2822 Section 3.4.1
		/// </summary>
		public const string dcontent = @"(?:[" + dtext + @"|" + quoted_pair + @")";
		/// <summary>
		/// RFC 2822 Section 3.4.1
		/// </summary>
		public const string addr_spec = local_part + "@" + domain;
	}
}
