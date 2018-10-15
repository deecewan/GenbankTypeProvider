namespace GenbankTypeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type GenbankTypeProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces (config)
    
    let ns = "Genbank.Provider"
    let asm = Assembly.GetExecutingAssembly()

    let createTypes () =
      let metaType = ProvidedTypeDefinition(asm, ns, "Meta", Some typeof<obj>)
      let metaAuthorProp = ProvidedProperty("Author", typeof<string>, isStatic = true, getterCode = (fun args -> <@@ "David Buchan-Swanson" @@>))
      metaType.AddMember(metaAuthorProp)

      Helpers.loadGenomeVariants()
      |> List.map(TypeGenerators.createGenomeVariantType(asm, ns))
      |> List.append([metaType])
    do
        this.AddNamespace(ns, createTypes())

[<assembly:TypeProviderAssembly>]
do ()