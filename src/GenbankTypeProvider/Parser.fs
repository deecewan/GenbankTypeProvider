module GenBankProvider.Parser

open Bio.IO.GenBank

let parse file = GenBankParser().Parse(file)
