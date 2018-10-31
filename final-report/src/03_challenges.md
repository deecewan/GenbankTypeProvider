# Challenges

### Wasted Network Requests
As highlighted in [Caching](#caching), the provider makes a lot of redundant API calls and does a lot of unused data processing if the types used are already explicitly defined. This makes sense from the point-of-view of a type provider, because the compiler has to assert that the types actually exist in order to verify correctness of the program.
It is currently unknown whether a type can be 'verified' rather than 'generated'. That is to say, given a complete type definition like

```fsharp
Genbank.Provider.Other.Genomes.``synthetic_Mycoplasma``.``Annotation Hashes``.``Assembly accession``
```
is it possible to verify that just that type is correct using the provider, or if every layer will have to be generated again. Given a type, the URL to lookup to verify if a type is valid can be determined by simply reversing the steps used to generate the type.

### Cross-Platform Development and Consumption
As noted previously, it is not currently possible to build the project, or any tool using the project, on a platform outside of Windows. This limits the development of the project, and hugely limits the target audience for consumption of the tool.
Establishing the problem, and fixing it, is important to ensure that the widest possible reach is available. First steps in this regard involve converting the project to the 'Project Scaffold' structure and changing out the build tool to FAKE.

### Linking of Genomes
A feature of Genbank files is that they contain a list of related genes.
Ideally, these genes should link, through the types, to a list of related
genomes. Establishing how to re-use types in this manner, and providing a
`siblings` or similar type on any given genome would greatly improve the usage
of the tool for the purposes of exploration.
