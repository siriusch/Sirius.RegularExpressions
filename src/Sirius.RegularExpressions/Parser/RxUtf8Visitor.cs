using System;

using Sirius.RegularExpressions.Invariant;

namespace Sirius.RegularExpressions.Parser {
	public class RxUtf8Visitor: IRegexVisitor<byte, object, RxNode<byte>> {
		public RxNode<byte> Accept(RxAccept<byte> token, object context) {
			throw new NotImplementedException();
		}

		public RxNode<byte> Alternation(RxAlternation<byte> token, object context) {
			throw new NotImplementedException();
		}

		public RxNode<byte> Concatenation(RxConcatenation<byte> token, object context) {
			throw new NotImplementedException();
		}

		public RxNode<byte> Empty(RxEmpty<byte> token, object context) {
			throw new NotImplementedException();
		}

		public RxNode<byte> Match(RxMatch<byte> token, object context) {
			throw new NotImplementedException();
		}

		public RxNode<byte> Quantified(RxQuantified<byte> token, object context) {
			throw new NotImplementedException();
		}
	}
}
