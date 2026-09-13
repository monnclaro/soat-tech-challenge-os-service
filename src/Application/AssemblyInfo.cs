using System.Runtime.CompilerServices;

// Handlers de domain event são internal ao assembly (mesmo padrão do monolito de
// origem) — o Scrutor os descobre via reflection (publicOnly: false) em runtime,
// mas os testes de unidade também precisam poder instanciá-los diretamente.
[assembly: InternalsVisibleTo("Tests")]
