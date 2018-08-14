using System;
using System.Linq;

using Sirius.RegularExpressions.Parser;
using Sirius.Unicode;

using Xunit;

namespace Sirius.RegularExpressions.RegularExpressions.Parser {
	public class UnicodeCharSetTest {
		[Theory]
		[InlineData(@"a", true, "L")]
		[InlineData(@"a", true, "Letter")]
		[InlineData(@"a", false, "Lu")]
		[InlineData(@"a", false, "Uppercase_Letter")]
		[InlineData(@"a", true, "Ll")]
		[InlineData(@"a", true, "Lowercase_Letter")]
		[InlineData(@"\u0000", true, "InBasic_Latin")]
		[InlineData(@"\u007F", true, "InBasic_Latin")]
		[InlineData(@"\u0080", true, "InLatin-1_Supplement")]
		[InlineData(@"\u00FF", true, "InLatin-1_Supplement")]
		[InlineData(@"\u0100", true, "InLatin_Extended-A")]
		[InlineData(@"\u017F", true, "InLatin_Extended-A")]
		[InlineData(@"\u0180", true, "InLatin_Extended-B")]
		[InlineData(@"\u024F", true, "InLatin_Extended-B")]
		[InlineData(@"\u0250", true, "InIPA_Extensions")]
		[InlineData(@"\u02AF", true, "InIPA_Extensions")]
		[InlineData(@"\u02B0", true, "InSpacing_Modifier_Letters")]
		[InlineData(@"\u02FF", true, "InSpacing_Modifier_Letters")]
		[InlineData(@"\u0300", true, "InCombining_Diacritical_Marks")]
		[InlineData(@"\u036F", true, "InCombining_Diacritical_Marks")]
		[InlineData(@"\u0370", true, "InGreek_and_Coptic")]
		[InlineData(@"\u03FF", true, "InGreek_and_Coptic")]
		[InlineData(@"\u0400", true, "InCyrillic")]
		[InlineData(@"\u04FF", true, "InCyrillic")]
		[InlineData(@"\u0500", true, "InCyrillic_Supplementary")]
		[InlineData(@"\u052F", true, "InCyrillic_Supplementary")]
		[InlineData(@"\u0530", true, "InArmenian")]
		[InlineData(@"\u058F", true, "InArmenian")]
		[InlineData(@"\u0590", true, "InHebrew")]
		[InlineData(@"\u05FF", true, "InHebrew")]
		[InlineData(@"\u0600", true, "InArabic")]
		[InlineData(@"\u06FF", true, "InArabic")]
		[InlineData(@"\u0700", true, "InSyriac")]
		[InlineData(@"\u074F", true, "InSyriac")]
		[InlineData(@"\u0780", true, "InThaana")]
		[InlineData(@"\u07BF", true, "InThaana")]
		[InlineData(@"\u0900", true, "InDevanagari")]
		[InlineData(@"\u097F", true, "InDevanagari")]
		[InlineData(@"\u0980", true, "InBengali")]
		[InlineData(@"\u09FF", true, "InBengali")]
		[InlineData(@"\u0A00", true, "InGurmukhi")]
		[InlineData(@"\u0A7F", true, "InGurmukhi")]
		[InlineData(@"\u0A80", true, "InGujarati")]
		[InlineData(@"\u0AFF", true, "InGujarati")]
		[InlineData(@"\u0B00", true, "InOriya")]
		[InlineData(@"\u0B7F", true, "InOriya")]
		[InlineData(@"\u0B80", true, "InTamil")]
		[InlineData(@"\u0BFF", true, "InTamil")]
		[InlineData(@"\u0C00", true, "InTelugu")]
		[InlineData(@"\u0C7F", true, "InTelugu")]
		[InlineData(@"\u0C80", true, "InKannada")]
		[InlineData(@"\u0CFF", true, "InKannada")]
		[InlineData(@"\u0D00", true, "InMalayalam")]
		[InlineData(@"\u0D7F", true, "InMalayalam")]
		[InlineData(@"\u0D80", true, "InSinhala")]
		[InlineData(@"\u0DFF", true, "InSinhala")]
		[InlineData(@"\u0E00", true, "InThai")]
		[InlineData(@"\u0E7F", true, "InThai")]
		[InlineData(@"\u0E80", true, "InLao")]
		[InlineData(@"\u0EFF", true, "InLao")]
		[InlineData(@"\u0F00", true, "InTibetan")]
		[InlineData(@"\u0FFF", true, "InTibetan")]
		[InlineData(@"\u1000", true, "InMyanmar")]
		[InlineData(@"\u109F", true, "InMyanmar")]
		[InlineData(@"\u10A0", true, "InGeorgian")]
		[InlineData(@"\u10FF", true, "InGeorgian")]
		[InlineData(@"\u1100", true, "InHangul_Jamo")]
		[InlineData(@"\u11FF", true, "InHangul_Jamo")]
		[InlineData(@"\u1200", true, "InEthiopic")]
		[InlineData(@"\u137F", true, "InEthiopic")]
		[InlineData(@"\u13A0", true, "InCherokee")]
		[InlineData(@"\u13FF", true, "InCherokee")]
		[InlineData(@"\u1400", true, "InUnified_Canadian_Aboriginal_Syllabics")]
		[InlineData(@"\u167F", true, "InUnified_Canadian_Aboriginal_Syllabics")]
		[InlineData(@"\u1680", true, "InOgham")]
		[InlineData(@"\u169F", true, "InOgham")]
		[InlineData(@"\u16A0", true, "InRunic")]
		[InlineData(@"\u16FF", true, "InRunic")]
		[InlineData(@"\u1700", true, "InTagalog")]
		[InlineData(@"\u171F", true, "InTagalog")]
		[InlineData(@"\u1720", true, "InHanunoo")]
		[InlineData(@"\u173F", true, "InHanunoo")]
		[InlineData(@"\u1740", true, "InBuhid")]
		[InlineData(@"\u175F", true, "InBuhid")]
		[InlineData(@"\u1760", true, "InTagbanwa")]
		[InlineData(@"\u177F", true, "InTagbanwa")]
		[InlineData(@"\u1780", true, "InKhmer")]
		[InlineData(@"\u17FF", true, "InKhmer")]
		[InlineData(@"\u1800", true, "InMongolian")]
		[InlineData(@"\u18AF", true, "InMongolian")]
		[InlineData(@"\u1900", true, "InLimbu")]
		[InlineData(@"\u194F", true, "InLimbu")]
		[InlineData(@"\u1950", true, "InTai_Le")]
		[InlineData(@"\u197F", true, "InTai_Le")]
		[InlineData(@"\u19E0", true, "InKhmer_Symbols")]
		[InlineData(@"\u19FF", true, "InKhmer_Symbols")]
		[InlineData(@"\u1D00", true, "InPhonetic_Extensions")]
		[InlineData(@"\u1D7F", true, "InPhonetic_Extensions")]
		[InlineData(@"\u1E00", true, "InLatin_Extended_Additional")]
		[InlineData(@"\u1EFF", true, "InLatin_Extended_Additional")]
		[InlineData(@"\u1F00", true, "InGreek_Extended")]
		[InlineData(@"\u1FFF", true, "InGreek_Extended")]
		[InlineData(@"\u2000", true, "InGeneral_Punctuation")]
		[InlineData(@"\u206F", true, "InGeneral_Punctuation")]
		[InlineData(@"\u2070", true, "InSuperscripts_and_Subscripts")]
		[InlineData(@"\u209F", true, "InSuperscripts_and_Subscripts")]
		[InlineData(@"\u20A0", true, "InCurrency_Symbols")]
		[InlineData(@"\u20CF", true, "InCurrency_Symbols")]
		[InlineData(@"\u20D0", true, "InCombining_Diacritical_Marks_for_Symbols")]
		[InlineData(@"\u20FF", true, "InCombining_Diacritical_Marks_for_Symbols")]
		[InlineData(@"\u2100", true, "InLetterlike_Symbols")]
		[InlineData(@"\u214F", true, "InLetterlike_Symbols")]
		[InlineData(@"\u2150", true, "InNumber_Forms")]
		[InlineData(@"\u218F", true, "InNumber_Forms")]
		[InlineData(@"\u2190", true, "InArrows")]
		[InlineData(@"\u21FF", true, "InArrows")]
		[InlineData(@"\u2200", true, "InMathematical_Operators")]
		[InlineData(@"\u22FF", true, "InMathematical_Operators")]
		[InlineData(@"\u2300", true, "InMiscellaneous_Technical")]
		[InlineData(@"\u23FF", true, "InMiscellaneous_Technical")]
		[InlineData(@"\u2400", true, "InControl_Pictures")]
		[InlineData(@"\u243F", true, "InControl_Pictures")]
		[InlineData(@"\u2440", true, "InOptical_Character_Recognition")]
		[InlineData(@"\u245F", true, "InOptical_Character_Recognition")]
		[InlineData(@"\u2460", true, "InEnclosed_Alphanumerics")]
		[InlineData(@"\u24FF", true, "InEnclosed_Alphanumerics")]
		[InlineData(@"\u2500", true, "InBox_Drawing")]
		[InlineData(@"\u257F", true, "InBox_Drawing")]
		[InlineData(@"\u2580", true, "InBlock_Elements")]
		[InlineData(@"\u259F", true, "InBlock_Elements")]
		[InlineData(@"\u25A0", true, "InGeometric_Shapes")]
		[InlineData(@"\u25FF", true, "InGeometric_Shapes")]
		[InlineData(@"\u2600", true, "InMiscellaneous_Symbols")]
		[InlineData(@"\u26FF", true, "InMiscellaneous_Symbols")]
		[InlineData(@"\u2700", true, "InDingbats")]
		[InlineData(@"\u27BF", true, "InDingbats")]
		[InlineData(@"\u27C0", true, "InMiscellaneous_Mathematical_Symbols-A")]
		[InlineData(@"\u27EF", true, "InMiscellaneous_Mathematical_Symbols-A")]
		[InlineData(@"\u27F0", true, "InSupplemental_Arrows-A")]
		[InlineData(@"\u27FF", true, "InSupplemental_Arrows-A")]
		[InlineData(@"\u2800", true, "InBraille_Patterns")]
		[InlineData(@"\u28FF", true, "InBraille_Patterns")]
		[InlineData(@"\u2900", true, "InSupplemental_Arrows-B")]
		[InlineData(@"\u297F", true, "InSupplemental_Arrows-B")]
		[InlineData(@"\u2980", true, "InMiscellaneous_Mathematical_Symbols-B")]
		[InlineData(@"\u29FF", true, "InMiscellaneous_Mathematical_Symbols-B")]
		[InlineData(@"\u2A00", true, "InSupplemental_Mathematical_Operators")]
		[InlineData(@"\u2AFF", true, "InSupplemental_Mathematical_Operators")]
		[InlineData(@"\u2B00", true, "InMiscellaneous_Symbols_and_Arrows")]
		[InlineData(@"\u2BFF", true, "InMiscellaneous_Symbols_and_Arrows")]
		[InlineData(@"\u2E80", true, "InCJK_Radicals_Supplement")]
		[InlineData(@"\u2EFF", true, "InCJK_Radicals_Supplement")]
		[InlineData(@"\u2F00", true, "InKangxi_Radicals")]
		[InlineData(@"\u2FDF", true, "InKangxi_Radicals")]
		[InlineData(@"\u2FF0", true, "InIdeographic_Description_Characters")]
		[InlineData(@"\u2FFF", true, "InIdeographic_Description_Characters")]
		[InlineData(@"\u3000", true, "InCJK_Symbols_and_Punctuation")]
		[InlineData(@"\u303F", true, "InCJK_Symbols_and_Punctuation")]
		[InlineData(@"\u3040", true, "InHiragana")]
		[InlineData(@"\u309F", true, "InHiragana")]
		[InlineData(@"\u30A0", true, "InKatakana")]
		[InlineData(@"\u30FF", true, "InKatakana")]
		[InlineData(@"\u3100", true, "InBopomofo")]
		[InlineData(@"\u312F", true, "InBopomofo")]
		[InlineData(@"\u3130", true, "InHangul_Compatibility_Jamo")]
		[InlineData(@"\u318F", true, "InHangul_Compatibility_Jamo")]
		[InlineData(@"\u3190", true, "InKanbun")]
		[InlineData(@"\u319F", true, "InKanbun")]
		[InlineData(@"\u31A0", true, "InBopomofo_Extended")]
		[InlineData(@"\u31BF", true, "InBopomofo_Extended")]
		[InlineData(@"\u31F0", true, "InKatakana_Phonetic_Extensions")]
		[InlineData(@"\u31FF", true, "InKatakana_Phonetic_Extensions")]
		[InlineData(@"\u3200", true, "InEnclosed_CJK_Letters_and_Months")]
		[InlineData(@"\u32FF", true, "InEnclosed_CJK_Letters_and_Months")]
		[InlineData(@"\u3300", true, "InCJK_Compatibility")]
		[InlineData(@"\u33FF", true, "InCJK_Compatibility")]
		[InlineData(@"\u3400", true, "InCJK_Unified_Ideographs_Extension_A")]
		[InlineData(@"\u4DBF", true, "InCJK_Unified_Ideographs_Extension_A")]
		[InlineData(@"\u4DC0", true, "InYijing_Hexagram_Symbols")]
		[InlineData(@"\u4DFF", true, "InYijing_Hexagram_Symbols")]
		[InlineData(@"\u4E00", true, "InCJK_Unified_Ideographs")]
		[InlineData(@"\u9FFF", true, "InCJK_Unified_Ideographs")]
		[InlineData(@"\uA000", true, "InYi_Syllables")]
		[InlineData(@"\uA48F", true, "InYi_Syllables")]
		[InlineData(@"\uA490", true, "InYi_Radicals")]
		[InlineData(@"\uA4CF", true, "InYi_Radicals")]
		[InlineData(@"\uAC00", true, "InHangul_Syllables")]
		[InlineData(@"\uD7AF", true, "InHangul_Syllables")]
		[InlineData(@"\uE000", true, "InPrivate_Use_Area")]
		[InlineData(@"\uF8FF", true, "InPrivate_Use_Area")]
		[InlineData(@"\uF900", true, "InCJK_Compatibility_Ideographs")]
		[InlineData(@"\uFAFF", true, "InCJK_Compatibility_Ideographs")]
		[InlineData(@"\uFB00", true, "InAlphabetic_Presentation_Forms")]
		[InlineData(@"\uFB4F", true, "InAlphabetic_Presentation_Forms")]
		[InlineData(@"\uFB50", true, "InArabic_Presentation_Forms-A")]
		[InlineData(@"\uFDFF", true, "InArabic_Presentation_Forms-A")]
		[InlineData(@"\uFE00", true, "InVariation_Selectors")]
		[InlineData(@"\uFE0F", true, "InVariation_Selectors")]
		[InlineData(@"\uFE20", true, "InCombining_Half_Marks")]
		[InlineData(@"\uFE2F", true, "InCombining_Half_Marks")]
		[InlineData(@"\uFE30", true, "InCJK_Compatibility_Forms")]
		[InlineData(@"\uFE4F", true, "InCJK_Compatibility_Forms")]
		[InlineData(@"\uFE50", true, "InSmall_Form_Variants")]
		[InlineData(@"\uFE6F", true, "InSmall_Form_Variants")]
		[InlineData(@"\uFE70", true, "InArabic_Presentation_Forms-B")]
		[InlineData(@"\uFEFF", true, "InArabic_Presentation_Forms-B")]
		[InlineData(@"\uFF00", true, "InHalfwidth_and_Fullwidth_Forms")]
		[InlineData(@"\uFFEF", true, "InHalfwidth_and_Fullwidth_Forms")]
		[InlineData(@"\uFFF0", true, "InSpecials")]
		[InlineData(@"\uFFFD", true, "InSpecials")]
		public void CharacterInSet(string ch, bool inSet, string unicodeName) {
			Codepoint parsedChar;
			if (ch.StartsWith("\\")) {
				Assert.True(((RangeSetHandle.Static)RegexMatchSet.ParseEscape(ch)).TryGetSingle(out parsedChar));
			} else {
				parsedChar = ch.Single();
			}
			Assert.Equal(inSet, UnicodeRanges.FromUnicodeName(unicodeName).Contains(parsedChar));
		}
	}
}
