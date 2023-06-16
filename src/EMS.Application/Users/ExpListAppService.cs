﻿using EMS.Expenses;
using EMS.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EMS.Users
{
    public class ExpListAppService : ApplicationService
    {
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly IRepository<Expense, Guid> _expenseRepository;

        public ExpListAppService(IRepository<Payment, Guid> paymentRepository, IRepository<Expense, Guid> expenseRepository)
        {
            _paymentRepository = paymentRepository;
            _expenseRepository = expenseRepository;
        }

        public async Task<List<ExpenseDto>> GetExpenseListAsync()
        {
            var expenseDto = new List<ExpenseDto>();
            var expenses = await _expenseRepository.GetListAsync();
            foreach (var expense in expenses)
            {
                var expDto = new ExpenseDto
                {
                    Id = expense.Id,
                    paidBy = expense.paidBy,
                    expense_amount = expense.expense_amount,
                    groupId = expense.groupId

                };
                expenseDto.Add(expDto);

            }
            return expenseDto;
        }

        public async Task<List<ExpListGetDto>> GetUnSetteledListAsync()
        {
            var payments = await _paymentRepository.GetListAsync(p => p.IsSettled == false);
            var expenseList = new List<ExpListGetDto>();

            if (payments.Count == 0)
            {
                ExpListGetDto expListGetDtos = new ExpListGetDto();
                expListGetDtos.Message = "You do not have any expenses yet";
                expenseList.Add(expListGetDtos);
                return expenseList;
            }

            foreach (var payment in payments)
            {
                var expListGetDto = new ExpListGetDto
                {
                    ExpenseId = payment.ExpenseId,
                    PaymentId = payment.Id,
                    OwnedBy = payment.OwnedBy,
                    Amount = payment.Amount,
                    
                };
                expenseList.Add(expListGetDto);

            }
            return expenseList;
        }
    }
}
