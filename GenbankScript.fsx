#r "D:/projects/deecewan/GenbankTypeProvider/GenbankTypeProvider/bin/Debug/Bio.Core.dll"
#r "D:/projects/deecewan/GenbankTypeProvider/GenbankTypeProvider/bin/Debug/netstandard.dll"

open System.Net
open System.IO.Compression
open Bio.IO.GenBank

let parseGenbankFile (url: string) =
  let req = WebRequest.Create(url)
  req.Method <- WebRequestMethods.Ftp.DownloadFile
  let res = req.GetResponse() :?> FtpWebResponse
  let stream = new GZipStream(res.GetResponseStream(), CompressionMode.Decompress)
  Bio.IO.GenBank.GenBankParser().Parse(stream)

let url = "ftp://ftp.ncbi.nlm.nih.gov/genomes/genbank/other/synthetic_bacterium_JCVI-Syn3.0/all_assembly_versions/GCA_001708325.2_ASM170832v2/GCA_001708325.2_ASM170832v2_genomic.gbff.gz"

let parsed = parseGenbankFile(url)

let printAllProperties (item: obj) =
  printfn "%s -> {" (item.GetType().FullName)
  item.GetType().GetProperties()
    |> Array.iter (fun prop ->
        if prop.CanRead then
            let value = prop.GetValue(item)
            printfn "[%s]: %O" prop.Name value
        else
            printfn "[%s]: ?" prop.Name)
  printfn "}"  

Seq.cast<Bio.ISequence>(parsed) |> Seq.iter(
  fun item ->
    // we expect Genbank to be in here, because we parsed from Genbank
    let genbank = item.Metadata.Item("GenBank") :?> GenBankMetadata
    printAllProperties genbank.Locus
    for property in genbank.GetType().GetProperties() do
      let value = property.GetValue(genbank)
      printfn "[%s]: %O" property.Name value
)

printfn "done"