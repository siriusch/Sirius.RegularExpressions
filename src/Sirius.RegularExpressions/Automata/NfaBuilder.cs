using System;
using System.Collections.Generic;

using Sirius.RegularExpressions.Invariant;

namespace Sirius.RegularExpressions.Automata {
	public sealed class NfaBuilder<TLetter>: IRegexVisitor<TLetter, NfaState<TLetter>, NfaState<TLetter>>
			where TLetter: struct, IEquatable<TLetter>, IComparable<TLetter> {
		public static Nfa<TLetter> Build(RxNode<TLetter> node) {
			var builder = new NfaBuilder<TLetter>();
			var endState = node.Visit(builder, builder.startState);
			return new Nfa<TLetter>(builder.states, builder.startState, endState);
		}

		private readonly NfaState<TLetter> startState;
		private readonly Dictionary<Id<NfaState<TLetter>>, NfaState<TLetter>> states = new Dictionary<Id<NfaState<TLetter>>, NfaState<TLetter>>();

		public NfaBuilder() {
			this.startState = this.Create();
		}

		NfaState<TLetter> IRegexVisitor<TLetter, NfaState<TLetter>, NfaState<TLetter>>.Accept(RxAccept<TLetter> node, NfaState<TLetter> context) {
			var target = this.Create(node.Symbol, node.AcceptPrecedence);
			node.Inner.Visit(this, context).AddEpsilonTransition(target);
			return target;
		}

		NfaState<TLetter> IRegexVisitor<TLetter, NfaState<TLetter>, NfaState<TLetter>>.Alternation(RxAlternation<TLetter> node, NfaState<TLetter> context) {
			var left = this.Create();
			context.AddEpsilonTransition(left);
			var right = this.Create();
			context.AddEpsilonTransition(right);
			var target = this.Create();
			node.Left.Visit(this, left).AddEpsilonTransition(target);
			node.Right.Visit(this, right).AddEpsilonTransition(target);
			return target;
		}

		NfaState<TLetter> IRegexVisitor<TLetter, NfaState<TLetter>, NfaState<TLetter>>.Concatenation(RxConcatenation<TLetter> node, NfaState<TLetter> context) {
			return node.Right.Visit(this, node.Left.Visit(this, context));
		}

		NfaState<TLetter> IRegexVisitor<TLetter, NfaState<TLetter>, NfaState<TLetter>>.Empty(RxEmpty<TLetter> node, NfaState<TLetter> context) {
			return context;
		}

		NfaState<TLetter> IRegexVisitor<TLetter, NfaState<TLetter>, NfaState<TLetter>>.Match(RxMatch<TLetter> node, NfaState<TLetter> context) {
			var target = this.Create();
			foreach (var range in node.Letters) {
				context.AddMatchTransition(range, target);
			}
			return target;
		}

		NfaState<TLetter> IRegexVisitor<TLetter, NfaState<TLetter>, NfaState<TLetter>>.Quantified(RxQuantified<TLetter> node, NfaState<TLetter> context) {
			for (var i = 0; i < node.Min; i++) {
				context = node.Inner.Visit(this, context);
			}
			var target = context;
			if (node.Max.HasValue) {
				for (var i = node.Min; i < node.Max.Value; i++) {
					target = node.Inner.Visit(this, context);
					context.AddEpsilonTransition(target);
					context = target;
				}
			} else {
				// kleene closure
				var innerStart = this.Create();
				context.AddEpsilonTransition(innerStart);
				target = this.Create();
				innerStart.AddEpsilonTransition(target);
				var innerEnd = node.Inner.Visit(this, innerStart);
				innerEnd.AddEpsilonTransition(innerStart);
			}
			return target;
		}

		private NfaState<TLetter> Create(SymbolId? acceptSymbol = null, int precedence = 0) {
			var id = new Id<NfaState<TLetter>>(this.states.Count);
			var state = new NfaState<TLetter>(id, acceptSymbol, precedence);
			this.states.Add(id, state);
			return state;
		}
	}
}
