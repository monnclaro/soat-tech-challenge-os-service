using System.Runtime.CompilerServices;

// Mesmo padrão do Application/AssemblyInfo.cs: alguns componentes de infra
// (ex.: DomainEventsDispatcher) são internal ao assembly, mas os testes de
// unidade precisam poder instanciá-los diretamente.
[assembly: InternalsVisibleTo("Tests")]
