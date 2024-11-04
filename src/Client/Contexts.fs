module Contexts

open Lit

type Memo = {
    memo: string
}

type MemoMsg =
    | SetMemo of string

let memoCtx = Lit.newContext<Memo * (MemoMsg -> unit)>  "memo"

