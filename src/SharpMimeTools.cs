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
	public class SharpMimeTools {
		private static log4net.ILog log  = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static System.String[] _date_formats = new System.String[] {
																@"dddd, d MMM yyyy H:m:s zzz", @"ddd, d MMM yyyy H:m:s zzz", @"d MMM yyyy H:m:s zzz",
																@"dddd, d MMM yy H:m:s zzz", @"ddd, d MMM yy H:m:s zzz", @"d MMM yy H:m:s zzz",
																@"dddd, d MMM yyyy H:m zzz", @"ddd, d MMM yyyy H:m zzz", @"d MMM yyyy H:m zzz",
																@"dddd, d MMM yy H:m zzz", @"ddd, d MMM yy H:m zzz", @"d MMM yy H:m zzz"
			};
		/// <summary>
		/// Parses a <see cref="System.Text.Encoding" /> from a charset name
		/// </summary>
		/// <param name="charset">charset to parse</param>
		/// <returns>A <see cref="System.Text.Encoding" /> that represents the given <c>charset</c></returns>
		public static System.Text.Encoding parseCharSet ( System.String charset ) {
			try {
				return System.Text.Encoding.GetEncoding (charset);
			} catch ( System.Exception ) {
				return null;
			}
		}
		/// <summary>
		/// Parse a rfc 2822 address specification. rfc 2822 section 3.4
		/// </summary>
		/// <param name="from">field body to parse</param>
		/// <returns>A <see cref="System.Collections.IEnumerable" /> collection of <see cref="anmar.SharpMimeTools.SharpMimeAddress" /></returns>
		public static System.Collections.IEnumerable parseFrom ( System.String from ) {
			return anmar.SharpMimeTools.SharpMimeAddressCollection.Parse (from);
		}
		/// <summary>
		/// Parse a rfc 2822 name-address specification. rfc 2822 section 3.4
		/// </summary>
		/// <param name="from">address</param>
		/// <param name="part">1 is display-name; 2 is addr-spec</param>
		/// <returns>the requested <see cref="System.String" /></returns>
		public static System.String parseFrom ( System.String from, int part ) {
			int pos;
			if ( from==null || from.Length<1) {
				return System.String.Empty;
			}
			switch (part) {
				case 1:
					pos = from.LastIndexOf('<');
					pos = (pos<0)?from.Length:pos;
					from = from.Substring (0, pos).Trim();
					from = anmar.SharpMimeTools.SharpMimeTools.parserfc2047Header ( from );
					return from;
				case 2:
					pos = from.LastIndexOf('<')+1;
					return from.Substring(pos, from.Length-pos).Trim(new char[]{'<','>',' '});
			}
			return from;
		}
		/// <summary>
		/// Parse a rfc 2822 date and time specification. rfc 2822 section 3.3
		/// </summary>
		/// <param name="date">rfc 2822 date-time</param>
		/// <returns>A <see cref="System.DateTime" /> from the parsed header body</returns>
		public static System.DateTime parseDate ( System.String date ) {
			if ( date==null || date.Equals(System.String.Empty) )
				return System.DateTime.MinValue;
			System.DateTime msgDateTime;
			date = anmar.SharpMimeTools.SharpMimeTools.uncommentString (date);
			msgDateTime = new System.DateTime (0);
			try {
				// TODO: Complete the list
				date = date.Replace("UT", "+0000");
				date = date.Replace("GMT", "+0000");
				date = date.Replace("EDT", "-0400");
				date = date.Replace("EST", "-0500");
				date = date.Replace("CDT", "-0500");
				date = date.Replace("MDT", "-0600");
				date = date.Replace("MST", "-0600");
				date = date.Replace("EST", "-0700");
				date = date.Replace("PDT", "-0700");
				date = date.Replace("PST", "-0800");

				date = date.Replace("AM", System.String.Empty);
				date = date.Replace("PM", System.String.Empty);
				int rpos = date.LastIndexOfAny(new Char[]{' ', '\t'});
				if (rpos>0 && rpos != date.Length - 6)
					date = date.Substring(0, rpos + 1) + "-0000";
				date = date.Insert(date.Length-2, ":");
				msgDateTime = DateTime.ParseExact(date, 
					_date_formats,
					System.Globalization.CultureInfo.CreateSpecificCulture("en-us"),
					System.Globalization.DateTimeStyles.AllowInnerWhite);
			} catch ( System.Exception e ) {
				msgDateTime = new System.DateTime (0);
				if ( log.IsErrorEnabled )
					log.Error(System.String.Concat("Error parsing date: [", date, "]"), e);
			}
			return msgDateTime;
		}
		/// <summary>
		/// Parse a rfc 2822 header field with parameters
		/// </summary>
		/// <param name="field">field name</param>
		/// <param name="fieldbody">field body to parse</param>
		/// <returns>A <see cref="System.Collections.Specialized.StringDictionary" /> from the parsed field body</returns>
		public static System.Collections.Specialized.StringDictionary parseHeaderFieldBody ( System.String field, System.String fieldbody ) {
			if ( fieldbody==null )
				return null;
			fieldbody = anmar.SharpMimeTools.SharpMimeTools.uncommentString (fieldbody);
			System.Collections.Specialized.StringDictionary fieldbodycol = new System.Collections.Specialized.StringDictionary ();
			System.String[] words = fieldbody.Split(new Char[]{';'});
			if ( words.Length>0 ) {
				fieldbodycol.Add (field.ToLower(), words[0].ToLower());
				for (int i=1; i<words.Length; i++ ) {
					System.String[] param = words[i].Trim(new Char[]{' ', '\t'}).Split(new Char[]{'='}, 2);
					if ( param.Length==2 ) {
						if ( param[1].StartsWith("\"") && !param[1].EndsWith("\"")) {
							do {
								param[1] += ";" + words[++i];
							} while  ( !words[i].EndsWith("\"") && i<words.Length);
						}
						fieldbodycol.Add ( param[0], anmar.SharpMimeTools.SharpMimeTools.parserfc2047Header (param[1].TrimEnd(';').Trim('\"')) );
					}
				}
			}
			return fieldbodycol;
		}
		/// <summary>
		/// Parse and decode rfc 2047 header body
		/// </summary>
		/// <param name="header">header body to parse</param>
		/// <returns>parsed <see cref="System.String" /></returns>
		public static System.String parserfc2047Header ( System.String header ) {
			header = header.Replace ("\"", System.String.Empty);
			header = anmar.SharpMimeTools.SharpMimeTools.rfc2047decode(header);
			return header;
		}
		/// <summary>
		/// Decode rfc 2047 definition of quoted-printable
		/// </summary>
		/// <param name="charset">charset to use when decoding</param>
		/// <param name="orig"><c>string</c> to decode</param>
		/// <returns>the decoded <see cref="System.String" /></returns>
		public static System.String QuotedPrintable2Unicode ( System.String charset, System.String orig ) {
			System.Text.Encoding enc = anmar.SharpMimeTools.SharpMimeTools.parseCharSet (charset);
			if ( enc==null || orig==null )
				return orig;
			anmar.SharpMimeTools.SharpMimeTools.QuotedPrintable2Unicode ( enc, ref orig );
			return orig;
		}
		/// <summary>
		/// Decode rfc 2047 definition of quoted-printable
		/// </summary>
		/// <param name="enc"><see cref="System.Text.Encoding" /> to use</param>
		/// <param name="orig"><c>string</c> to decode</param>
		public static void QuotedPrintable2Unicode ( System.Text.Encoding enc, ref System.String orig ) {
			if ( enc==null || orig==null )
				return;

			System.Text.StringBuilder decoded = new System.Text.StringBuilder(orig);
			int i = 0;
			System.String hexNumber;
			System.Byte[] ch = new System.Byte[1];
			while ( i < decoded.Length - 2 ) {
				System.String decodedItem = null;
				if ( decoded[i] == '=' ) {
					hexNumber = decoded.ToString(i+1, 2);
					if ( hexNumber.Equals(ABNF.CRLF) ) {
						decodedItem = System.String.Empty;
					} else {
						try {
							//TODO: this ugly workaround should disapear
							ch[0] = System.Convert.ToByte(hexNumber, 16);
							decodedItem = enc.GetString ( ch );
						} catch ( System.Exception ) {}
					}
					if ( decodedItem!=null )
						decoded.Replace( "=" + hexNumber, decodedItem );
				}
				if ( decodedItem!=null )
					i+=decodedItem.Length;
				else
					i++;
			}
			orig = decoded.ToString();
			return;
		}
		/// <summary>
		/// rfc 2047 header body decoding
		/// </summary>
		/// <param name="word"><c>string</c> to decode</param>
		/// <returns>the decoded <see cref="System.String" /></returns>
		public static System.String rfc2047decode ( System.String word ) {
			System.String[] words;
			System.String[] wordetails;

			System.Text.RegularExpressions.Regex rfc2047format = new System.Text.RegularExpressions.Regex (@"(?:\s*(=\?[\-a-zA-Z0-9]+\?[qQbB]\?[a-zA-Z0-9=_\-\.$%&/\'\\!:;{}\+\*\|@#~`^]+\?=)\s*)+", System.Text.RegularExpressions.RegexOptions.ECMAScript);
			// No rfc2047 format
			if ( !rfc2047format.IsMatch (word) ){
				if ( log.IsDebugEnabled )
					log.Debug ("Not a RFC 2047 string: " + word);
				return word;
			}
			if ( log.IsDebugEnabled )
				log.Debug ("Decoding 2047 string: " + word);
			words = rfc2047format.Split ( word );
			word = System.String.Empty;
			rfc2047format = new System.Text.RegularExpressions.Regex (@"=\?([\-a-zA-Z0-9]+)\?([qQbB])\?([a-zA-Z0-9=_\-\.$%&/\'\\!:;{}\+\*\|@#~`^]+)\?=", System.Text.RegularExpressions.RegexOptions.ECMAScript);
			for ( int i=0; i<words.GetLength (0); i++ ) {
				if ( !rfc2047format.IsMatch (words[i]) ){
					word += words[i];
					continue;
				}
				wordetails = rfc2047format.Split ( words[i] );

				switch (wordetails[2]) {
					case "q":
					case "Q":
						word += anmar.SharpMimeTools.SharpMimeTools.QuotedPrintable2Unicode ( wordetails[1], wordetails[3] ).Replace ('_', ' ');;
						break;
					case "b":
					case "B":
						try {
							System.Text.Encoding enc = System.Text.Encoding.GetEncoding (wordetails[1]);
							System.Byte[] ch = System.Convert.FromBase64String(wordetails[3]);
							word += enc.GetString (ch);
						} catch ( System.Exception ) {
						}
						break;
				}
			}
			if ( log.IsDebugEnabled )
				log.Debug ("Decoded 2047 string: " + word);
			return word;
		}
		/// <summary>
		/// Remove rfc 2822 comments
		/// </summary>
		/// <param name="fieldValue"><c>string</c> to uncomment</param>
		/// <returns></returns>
		// TODO: refactorize this
		public static System.String uncommentString ( System.String fieldValue ) {
			if ( fieldValue==null || fieldValue.Equals(System.String.Empty) )
				return fieldValue;
			if ( fieldValue.IndexOf('(')==-1 )
				return fieldValue.Trim();
			const int a = 0;
			const int b = 1;
			const int c = 2;

			System.Text.StringBuilder buf = new System.Text.StringBuilder();
			int leftSqureCount = 0;
			bool isQuotedPair = false;
			int state = a;

			for (int i = 0; i < fieldValue.Length; i ++) {
				switch (state) {
					case a:
						if (fieldValue[i] == '"') {
							state = c;
							System.Diagnostics.Debug.Assert(!isQuotedPair, "quoted-pair");
						}
						else if (fieldValue[i] == '(') {
							state = b;
							leftSqureCount ++;
							System.Diagnostics.Debug.Assert(!isQuotedPair, "quoted-pair");
						}
						break;
					case b:
						if (!isQuotedPair) {
							if (fieldValue[i] == '(')
								leftSqureCount ++;
							else if (fieldValue[i] == ')') {
								leftSqureCount --;
								if (leftSqureCount == 0) {
									buf.Append(' ');
									state = a;
									continue;
								}
							}
						}
						break;
					case c:
						if (!isQuotedPair) {
							if (fieldValue[i] == '"')
								state = a;
						}
						break;
					default:
						break;
				}

				if (state != a) { //quoted-pair
					if (isQuotedPair)
						isQuotedPair = false;
					else
						isQuotedPair = fieldValue[i] == '\\';
				}
				if (state != b)
					buf.Append(fieldValue[i]);
			}
      
			return buf.ToString().Trim();

		}
	}
}
