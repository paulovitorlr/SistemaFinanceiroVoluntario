# Sistema de Controle Financeiro para Festejos Comunitários

> Projeto voluntário desenvolvido para auxiliar no controle do fluxo de caixa durante os festejos da minha comunidade.

## 📖 Sobre o projeto

Este projeto nasceu da necessidade de organizar as vendas realizadas durante os festejos da minha localidade.

O objetivo é substituir o controle manual por uma aplicação simples, rápida e que possa ser utilizada mesmo sem acesso à internet, facilitando o registro das vendas e o fechamento do caixa ao final de cada evento.

Após o levantamento inicial de requisitos, foi utilizado Inteligência Artificial para acelerar a criação de um protótipo funcional. A partir dessa base, o projeto está sendo revisado, refatorado e evoluído manualmente, buscando melhorar a arquitetura, organização do código, usabilidade e manutenção da aplicação.

---

## 🎯 Objetivos

- Registrar vendas rapidamente
- Controlar o fluxo de caixa
- Calcular troco automaticamente
- Gerar um fechamento detalhado do evento
- Permitir funcionamento totalmente offline
- Facilitar backup dos dados

---

## 🛠 Tecnologias

- C#
- .NET 8
- Windows Forms
- SQLite
- ADO.NET

---

## 📂 Estrutura do projeto

```
SistemaFinanceiro/
│
├── Data/
│   └── DatabaseService.cs
│
├── Models/
│   ├── Produto.cs
│   ├── ItemPedido.cs
│   └── ResumoFechamento.cs
│
├── MainForm.cs
│
└── README.md
```

### Organização

**MainForm.cs**

Responsável pela interface da aplicação, contendo as abas:

- Vendas
- Produtos
- Fechamento

---

**DatabaseService.cs**

Camada responsável pela persistência dos dados utilizando SQLite local.

Toda a aplicação funciona sem necessidade de servidor ou conexão com a internet.

---

**Models**

Contém as entidades utilizadas pelo sistema:

- Produto
- ItemPedido
- ResumoFechamento

---

## 🚀 Funcionalidades

### Venda de produtos

- Cardápio em formato de botões
- Inclusão rápida de itens
- Soma automática do pedido
- Cálculo automático do troco
- Registro da venda

---

### Cadastro de produtos

Permite cadastrar e editar os produtos vendidos durante o evento.

---

### Fechamento de caixa

Ao final do evento o sistema apresenta:

- Total vendido
- Quantidade de itens vendidos
- Valor recebido
- Troco devolvido
- Lucro estimado
- Produto mais vendido
- Relatório por produto

---

### Exportação

- Exportação dos dados em CSV

---

### Reinício do caixa

É possível zerar apenas as vendas do evento sem apagar o cadastro dos produtos.

---

## 💾 Banco de dados

O sistema utiliza SQLite em um único arquivo local.

```
%APPDATA%\CaixaFestejos\caixa.db
```

Essa abordagem oferece algumas vantagens:

- Não necessita servidor
- Não necessita internet
- Backup extremamente simples
- Basta copiar o arquivo para outro computador ou pendrive

---

## 📌 Estado atual do projeto

O projeto encontra-se em evolução.

A primeira versão foi construída rapidamente com auxílio de Inteligência Artificial para validar a ideia e disponibilizar um protótipo funcional.

Agora o desenvolvimento segue com foco em:

- Revisão completa do código
- Refatoração
- Melhoria da arquitetura
- Organização em camadas
- Boas práticas de desenvolvimento
- Correção de possíveis problemas
- Inclusão de novas funcionalidades

O objetivo é transformar o protótipo em uma aplicação mais robusta, organizada e de fácil manutenção.

---

## 📈 Melhorias planejadas

- [ ] Refatoração da arquitetura
- [ ] Separação de responsabilidades
- [ ] Camada de serviços
- [ ] Melhor organização do acesso a dados
- [ ] Melhor tratamento de erros
- [ ] Relatórios mais completos
- [ ] Impressão de comprovantes
- [ ] Backup automático
- [ ] Melhor experiência do usuário

---

## 📄 Licença

Este projeto foi desenvolvido de forma voluntária para atender uma necessidade da comunidade e também como oportunidade de aprendizado e evolução técnica.