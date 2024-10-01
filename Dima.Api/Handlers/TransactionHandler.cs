﻿using Dima.Api.Data;
using Dima.Core.Common.Extensions;
using Dima.Core.Handlers;
using Dima.Core.Models;
using Dima.Core.Requests.Transactions;
using Dima.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace Dima.Api.Handlers
{
    public class TransactionHandler(AppDbContext context) : ITransactionHandler
    {
        public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
        {
            try
            {
                var transaction = new Transaction
                {
                    UserId = request.UserId,
                    CategoryId = request.CategoryId,
                    CreatedAt = DateTime.Now,
                    Amount = request.Amount,
                    PaidOrReceivedAt = request.PaidOrReceiveAt,
                    Title = request.Title,
                    Type = request.Type
                };

                await context.Transactions.AddAsync(transaction);
                await context.SaveChangesAsync();

                return new Response<Transaction?>(transaction, 201, "Transação criada com sucesso.");
            }
            catch
            {
                return new Response<Transaction?>(null, 500, "Não foi possivel criar a transação");
            }
        }

        public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
        {
            try
            {
                var transaction = await context.Transactions
                    .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

                if (transaction is null)
                    return new Response<Transaction?>(null, 404, "Transação não encontrada");

                transaction.CategoryId = request.CategoryId;
                transaction.Amount = request.Amount;
                transaction.Title = request.Title;
                transaction.Type = request.Type;
                transaction.PaidOrReceivedAt = request.PaidOrReceiveAt;

                context.Transactions.Update(transaction);
                await context.SaveChangesAsync();

                return new Response<Transaction?>(transaction, message: "Transação atualizada com sucesso.");
            }
            catch
            {
                return new Response<Transaction?>(null, 500, "Não foi possivel atualizar a transação");
            }

        }

        public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
        {
            try
            {
                var transaction = await context.Transactions
                    .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

                if (transaction is null)
                    return new Response<Transaction?>(null, 404, "Transação não encontrada");

                context.Transactions.Remove(transaction);
                await context.SaveChangesAsync();

                return new Response<Transaction?>(transaction, message: "Transação excluída com sucesso.");
            }
            catch
            {
                return new Response<Transaction?>(null, 500, "Não foi possivel excluir a transação");
            }
        }

        public async Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request)
        {
            try
            {
                var transaction = await context.Transactions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == request.UserId);

                return transaction is null 
                    ?  new Response<Transaction?>(null, 404, "Transação não encontrada")
                    :  new Response<Transaction?>(transaction);
            }
            catch
            {
                return new Response<Transaction?>(null, 500, "Não foi possivel recuperar a transação");
            }
        }

        public async Task<PagedResponse<List<Transaction>?>> GetByPeriodAsync(GetTransactionByPeriodRequest request)
        {
            try
            {
                request.StartDate ??= DateTime.Now.GetFisrtDay();
                request.EndDate ??= DateTime.Now.GetLastDate();
            }
            catch
            {
                return new PagedResponse<List<Transaction>?> (null, 500, "Não foi possivel determinar a data de início ou termino");
            }

            try
            {
                var query = context.Transactions
                .AsNoTracking()
                .Where(x => x.CreatedAt >= request.StartDate &&
                x.CreatedAt <= request.EndDate &&
                x.UserId == request.UserId)
                .OrderBy(x => x.CreatedAt);

                var count = await query.CountAsync();

                var transactions = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                return new PagedResponse<List<Transaction>?>(transactions, count, request.PageNumber, request.PageSize);
            }
            catch
            {
                return new PagedResponse<List<Transaction>?>(null, 500, "Não foi possivel obter as transações.");
            }
        }
    }
}
