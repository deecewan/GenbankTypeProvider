module Test

open GenbankTypeProvider

let bacteria = ["Abditibacterium_utsteinense"; "Abiotrophia_defectiva"; "Abiotrophia_sp._HMSC24B09";]

for bacterium in bacteria do
  Helpers.getLatestAssemblyFor bacterium |>
    List.map(fun x -> System.Console.WriteLine("Item: {0}", x)) |> ignore
;;
