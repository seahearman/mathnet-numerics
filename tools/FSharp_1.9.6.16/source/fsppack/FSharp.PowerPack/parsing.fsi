//==========================================================================
// (c) Microsoft Corporation 2005-2008.   The interface to the module 
// is similar to that found in versions of other ML implementations, 
// but is not an exact match.  The type signatures in this interface
// are an edited version of those generated automatically by running 
// "bin\fsc.exe -i" on the implementation file.
//===========================================================================

/// Parsing: parser support for parsers produced by fsyacc.
///
/// Parsers generated by fsyacc provide location information within parser
/// actions.  However that information is not available globally, but
/// rather is accessed via the functions available on the following local
/// variable which is available in all parser actions:
///
///    parseState : 'a Microsoft.FSharp.Text.Parsing.IParseState
///
/// However, this is not compatible with the parser specifications used
/// with ocamlyacc and similar tools, which make a single parser state available
/// globally.  If you wish to use a global parser state (e.g. so your code will
/// cross-compile with OCaml) then you can use the functions in this file.
/// You will need to either generate the parser with '--ml-compatibility' option 
/// or add the code
///
///       Parsing.set_parse_state parseState;
///
/// at the start of each action of your grammar.  The functions below
/// simply report the results of corresponding calls to the latest object
/// specified by a call to set_parse_state.
///
/// Note that there could be unprotected multi-threaded concurrent access for the
/// parser information, so you should not in general use these
/// functions if there may be more than one parser active, and
/// should instead use the functions directly available from the parseState
/// object.
#if INTERNALIZED_POWER_PACK
module internal Internal.Utilities.Compatibility.OCaml.Parsing
open Internal.Utilities
open Internal.Utilities.Text.Parsing
#else
[<OCamlCompatibility("Consider using the Microsoft.FSharp.Text.Parsing namespace directly")>]
module Microsoft.FSharp.Compatibility.OCaml.Parsing
open Microsoft.FSharp.Text.Parsing
open Microsoft.FSharp.Compatibility.OCaml
#endif

val rhs_end: int -> int
val rhs_end_pos: int -> Lexing.position
val rhs_start: int -> int
val rhs_start_pos: int -> Lexing.position
val symbol_end: unit -> int
val symbol_end_pos: unit -> Lexing.position
val symbol_start: unit -> int
val symbol_start_pos: unit -> Lexing.position

val set_parse_state: IParseState -> unit

/// You can initialize error recovery by raising the Parse_error exception. 

exception Parse_error = Microsoft.FSharp.Text.Parsing.RecoverableParseError


