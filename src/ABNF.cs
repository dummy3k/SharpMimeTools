using System;

namespace anmar.SharpMimeTools
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class ABNF {
		public const string CRLF = "\r\n";
		public const string ALPHA = @"A-Za-z";
		public const string DIGIT = @"0-9";
		// RFC 2822 Section 2.2.2
		public const string WSP = @"\x20\x09";
		// RFC 2822 Section 3.2.1
		public const string NO_WS_CTL = @"\x01-\x08\x0B\x0C\x0E-\x1F\x7F";
		// FIXME: add obs-text
		public const string text = @"\x01-\x09\x0B\x0C\x0E-\x7F";
		// RFC 2822 Section 3.2.2
		// FIXME: add obs-qp
		public const string quoted_pair = @"\x5C[" + text + "]";
		// RFC 2822 Section 3.2.3
		// FIXME: add obs-FWS
		public const string FWS = @"(([" + WSP + @"]{0,}\r\n){0,1}[" + WSP + @"]+)";
		// FIXME: Correct this simplification
		public const string CFWS = FWS;
		// RFC 2822 Section 3.2.4
		public const string atext = ALPHA + DIGIT + @"\x21\x23-\x27\x2A\x2B\x2D\x2F\x3D\x3F\x5E\x5F\x60\x7B-\x7E";
		public const string atom = @"[" + atext + @"]+";
		public const string dot_atom = CFWS + @"{0,1}" + dot_atom_text + CFWS + @"{0,1}";
		public const string dot_atom_text = @"[" + atext + @"]{1,}(\.[" + atext + @"]{1,}){0,}";
		// RFC 2822 Section 3.2.5
		public const string DQUOTE = @"\x22";
		public const string qtext = NO_WS_CTL + @"\x21\x23-\x5B\x5D-\x7E";
		public const string qcontent = @"([" + qtext + @"|" + quoted_pair + @")";
		public const string quoted_string = CFWS + @"{0,1}" + DQUOTE + @"(" + FWS + @"{0,1}" + qcontent + "){0,}" + FWS + @"{0,1}" + DQUOTE + CFWS + @"{0,1}";
		// RFC 2822 Section 3.2.6
		public const string word = @"(" + atom + @"|" + quoted_string + @")";
		// RFC 2822 Section 3.4
		// FIXME: add obs-angle-addr
		public const string angle_addr = @"\x3C" + addr_spec + @"\x3E";
		// RFC 2822 Section 3.4.1
		// FIXME: add obs-local-part
		public const string local_part = @"(" + dot_atom + @"|" + quoted_string + @")";
		// FIXME: add obs-domain
		public const string domain = @"(" + dot_atom + @"|" + domain_literal + @")";
		public const string domain_literal = CFWS + @"{0,1}" + @"\[(" + FWS + @"{0,1}" + dcontent + @"){0,}" + FWS + @"{0,1}" + @"\]" + CFWS + @"{0,1}";
		public const string dtext = NO_WS_CTL + @"\x21-\x5A\x5E-\x7E";
		public const string dcontent = @"([" + dtext + @"|" + quoted_pair + @")";
		public const string addr_spec = local_part + "@" + domain;
	}
}
