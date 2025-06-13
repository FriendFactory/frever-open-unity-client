// It provides access of internal objects from Installer assembly
// and we can use that for accessing from Test assembly, if we will have it 

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Installers")]
[assembly: InternalsVisibleTo("Tests.EditMode")]