# Caixa dos Festejos — versão Windows Forms (offline)

Sistema de caixa para os festejos, agora como aplicativo Windows nativo.
Roda 100% offline: o banco de dados é um arquivo SQLite local, sem precisar
de internet nem de instalar servidor de banco nenhum.

## O que muda em relação à versão web

- É um `.exe` de verdade, aberto no Windows como qualquer programa.
- Os dados ficam salvos em:
  `C:\Users\<seu usuário>\AppData\Roaming\CaixaFestejos\caixa.db`
- Funciona sem internet, sem navegador, sem depender de nenhum servidor.
- Mesmas 3 abas: **Vender**, **Produtos**, **Fechamento** — mesma lógica que
  você já aprovou na versão web.

## Como abrir e rodar (passo a passo)

Você vai precisar de um computador Windows com o **.NET 8 SDK** instalado
(gratuito, da Microsoft). Se ainda não tiver:

1. Baixe em: https://dotnet.microsoft.com/download/dotnet/8.0
   (escolha o instalador "SDK", não o "Runtime")
2. Instale normalmente (next, next, finish).

### Opção A — usando Visual Studio (mais fácil, com botão de play)

1. Instale o **Visual Studio Community** (gratuito):
   https://visualstudio.microsoft.com/pt-br/vs/community/
   — na instalação, marque a carga de trabalho **"Desenvolvimento para
   desktop com .NET"**.
2. Abra a pasta `CaixaFestejos` no Visual Studio
   (Arquivo → Abrir → Pasta, ou dê duplo clique no `CaixaFestejos.csproj`).
3. Aperte **F5** (ou o botão verde "Iniciar"). Pronto, o programa abre.

### Opção B — usando só o terminal (sem instalar Visual Studio)

1. Abra o **Prompt de Comando** ou **PowerShell** dentro da pasta do projeto.
2. Rode:
   ```
   dotnet run
   ```
3. Na primeira vez, ele baixa a dependência do SQLite automaticamente
   (precisa de internet só nesse passo único de preparar o projeto).

### Gerando um .exe único para usar sem instalar nada depois

Depois que tiver testado e estiver satisfeito, gere um executável que
roda sozinho, sem precisar do SDK instalado na máquina que for usar no
dia do festejo:

```
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

O `.exe` final vai aparecer em:
`bin\Release\net8.0-windows\win-x64\publish\CaixaFestejos.exe`

Esse único arquivo pode ser copiado para qualquer computador Windows
(até um sem .NET instalado) e vai funcionar direto, offline, com o banco
de dados sendo criado automaticamente na primeira vez que abrir.

## Estrutura do projeto

```
CaixaFestejos/
├── CaixaFestejos.csproj      → arquivo do projeto
├── Program.cs                 → ponto de entrada
├── MainForm.cs                → a janela principal (3 abas)
├── Data/
│   └── DatabaseService.cs     → toda a parte de banco de dados (SQLite)
└── Models/
    ├── Produto.cs
    ├── ItemPedido.cs
    └── ResumoFechamento.cs
```

## Fazendo backup dos dados

Como tudo fica em um único arquivo (`caixa.db`), fazer backup é simples:
basta copiar esse arquivo para um pendrive ou nuvem depois do festejo.
Ele fica em:

```
%APPDATA%\CaixaFestejos\caixa.db
```

(Cole esse caminho na barra de endereço do Explorer do Windows para
chegar direto na pasta.)

## Próximos ajustes possíveis

- Separar forma de pagamento (dinheiro / Pix) nos relatórios.
- Tela de login por operador de caixa.
- Atalhos de teclado para vender mais rápido sem usar o mouse.

Qualquer ajuste, é só pedir.
