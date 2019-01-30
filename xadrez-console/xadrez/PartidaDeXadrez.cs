﻿using System.Collections.Generic;
using tabuleiro;

namespace xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro Tabuleiro { get; private set; }
        public int Turno { get; private set; }
        public Cor JogadorAtual { get; private set; }
        public bool Terminada { get; private set; }
        public HashSet<Peca> Pecas { get; private set; }
        public HashSet<Peca> Capturadas { get; private set; }
        public bool Xeque { get; private set; }

        public PartidaDeXadrez()
        {
            Tabuleiro = new Tabuleiro(8,8);
            Turno = 1;
            JogadorAtual = Cor.Branca;
            Terminada = false;
            Xeque = false;
            Pecas = new HashSet<Peca>();
            Capturadas = new HashSet<Peca>();
            colocarPecas();
        }

        public Peca executaMovimento(Posicao origem, Posicao destino)
        {
            Peca p = Tabuleiro.retirarPeca(origem);
            p.incrementarQteMovimentos();
            Peca pecaCapturada = Tabuleiro.retirarPeca(destino);
            Tabuleiro.colocarPeca(p, destino);
            if(pecaCapturada != null)
            {
                Capturadas.Add(pecaCapturada);
            }
            return pecaCapturada;
        }

        public void desfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca p = Tabuleiro.retirarPeca(destino);
            p.decrementarQteMovimentos();
            if(pecaCapturada != null)
            {
                Tabuleiro.colocarPeca(pecaCapturada, destino);
                Capturadas.Remove(pecaCapturada);
            }
            Tabuleiro.colocarPeca(p, origem);
        }

        public void realizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = executaMovimento(origem, destino);

            if(estaEmXeque(JogadorAtual))
            {
                desfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode ser por em Xeque!");
            }
            if(estaEmXeque(adversaria(JogadorAtual)))
            {
                Xeque = true;
            }
            else
            {
                Xeque = false;
            }
            if (testeXequeMate(adversaria(JogadorAtual)))
            {
                Terminada = true;
            }
            else
            {
                Turno++;
                mudaJogador();
            }
        }

        public void validarPosicaoDeOrigem(Posicao pos)
        {
            if(Tabuleiro.peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição de origem escolhida!");
            }
            if(JogadorAtual != Tabuleiro.peca(pos).Cor)
            {
                throw new TabuleiroException("A peça de origem escolhida não é sua!");
            }
            if(!Tabuleiro.peca(pos).existeMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
            }
        }

        public void validarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if(!Tabuleiro.peca(origem).movimentoPossivel(destino))
            {
                throw new TabuleiroException("Posição de destino inválida!");
            }
        }

        private void mudaJogador()
        {
            if(JogadorAtual == Cor.Branca)
            {
                JogadorAtual = Cor.Preta;
            }
            else
            {
                JogadorAtual = Cor.Branca;
            }

        }

        public HashSet<Peca> pecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach(Peca x in Capturadas)
            {
                if(x.Cor == cor)
                {
                    aux.Add(x);
                }
            }
            return aux;
        }

        public HashSet<Peca> pecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach(Peca x in Pecas)
            {
                if(x.Cor == cor)
                {
                    aux.Add(x);
                }
            }
            aux.ExceptWith(pecasCapturadas(cor));
            return aux;
        }

        private Peca rei(Cor cor)
        {
            foreach (Peca x in pecasEmJogo(cor))
            {
                if(x is Rei)
                {
                    return x;
                }
            }
            return null;
        }

        private Cor adversaria(Cor cor)
        {
            if(cor == Cor.Branca)
            {
                return Cor.Preta;
            }
            else
            {
                return Cor.Branca;
            }
        }

        public bool estaEmXeque(Cor cor)
        {
            Peca R = rei(cor);

            foreach(Peca x in pecasEmJogo(adversaria(cor)))
            {
                bool[,] mat = x.movimentosPossiveis();
                if(mat[R.Posicao.Linha, R.Posicao.Coluna])
                {
                    return true;
                }
            }
            return false;
        }

        public bool testeXequeMate(Cor cor)
        {
            if(!estaEmXeque(cor))
            {
                return false;
            }
            foreach(Peca x in pecasEmJogo(cor))
            {
                bool[,] mat = x.movimentosPossiveis();
                for(int i=0; i<Tabuleiro.Linhas; i++)
                {
                    for(int j=0; j<Tabuleiro.Colunas; j++)
                    {
                        if(mat[i,j])
                        {
                            Posicao origem = x.Posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = executaMovimento(origem, destino);
                            bool testeXeque = estaEmXeque(cor);
                            desfazMovimento(origem, destino, pecaCapturada);
                            if(!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void colocarNovaPeca(char coluna, int linha, Peca peca)
        {
            Tabuleiro.colocarPeca(peca, new PosicaoXadrez(coluna, linha).toPosicao());
            Pecas.Add(peca);
        }

        private void colocarPecas()
        {
            colocarNovaPeca('c', 1, new Torre(Tabuleiro, Cor.Branca));
            colocarNovaPeca('h', 7, new Torre(Tabuleiro, Cor.Branca));
            colocarNovaPeca('d', 1, new Rei(Tabuleiro, Cor.Branca));

            colocarNovaPeca('b', 8, new Torre(Tabuleiro, Cor.Preta));
            colocarNovaPeca('h', 8, new Torre(Tabuleiro, Cor.Preta));
            colocarNovaPeca('a', 8, new Rei(Tabuleiro, Cor.Preta));
        }
    }
}
