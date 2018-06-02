namespace GenbankTypeProvider

open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Reflection

[<TypeProvider>]
type GenbankTypeProvider (config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces (config)
    
    let ns = "Genbank.Provider"
    let asm = Assembly.GetExecutingAssembly()

    let attachBacteria (bacteriaRoot: ProvidedTypeDefinition) =
      (* It currently takes too long to load these from the FTP server - haven't come up with a caching strategy yet*)
      //let contents = Helpers.loadDirectoryFromFTP(Helpers.GENOME_BASE_URL)
      //let bacteria = [
      //  for c in contents do
      //    if c.Length > 1 then
      //      if c.[0].StartsWith("d") then yield Seq.last c
      //]

      // we're going to hard-code a list of bacterium in here for now

      TypeGenerators.createBacteriaTypes |> bacteriaRoot.AddMembers

    let createTypes () =
      let metaType = ProvidedTypeDefinition(asm, ns, "Meta", Some typeof<obj>)
      let metaAuthorProp = ProvidedProperty("Author", typeof<string>, isStatic = true, getterCode = (fun args -> <@@ "David Buchan-Swanson" @@>))
      metaType.AddMember(metaAuthorProp)

      let bacteriaType = ProvidedTypeDefinition(asm, ns, "Bacteria", Some typeof<obj>)
      attachBacteria(bacteriaType)

      [metaType; bacteriaType]
    do
        this.AddNamespace(ns, createTypes())

[<assembly:TypeProviderAssembly>]
do ()