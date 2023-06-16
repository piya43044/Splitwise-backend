﻿using EMS.Friends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using EMS.Expenses;
using EMS.Groups;
using Microsoft.AspNetCore.Authorization;

namespace EMS.Payments
{
    public class UserOutstandingDetailsAppService : ApplicationService, IUserOutstandingDetailsAppService
    {
        private readonly IRepository<Payment, Guid> _paymentRepository;
        private readonly IRepository<Expense, Guid> _expenseRepository;
        private readonly IRepository<Group, Guid> _groupRepository;
        private ICurrentUser _currentUser;

        public UserOutstandingDetailsAppService(IRepository<Payment, Guid> paymentRepository, IRepository<Expense, Guid> expenseRepository, IRepository<Group, Guid> groupRepository, ICurrentUser currentUser)
        {
            _paymentRepository = paymentRepository;
            _expenseRepository = expenseRepository;
            _groupRepository = groupRepository;
            _currentUser = currentUser;
        }
        public async Task<List<PaymentReturnDto>> GetPaymentInfoForCurrentUserAsync()
        {
            List<PaymentReturnDto> paymentReturns = new List<PaymentReturnDto>();

           // var currentUserId = _currentUser.Id;
            var currentUserId = new Guid("3a0bc559-4a98-1dfa-317d-c94539d43a69");

            var payments = await _paymentRepository.GetListAsync(p => p.OwnedBy == currentUserId && p.IsSettled == false);

            if (payments.Count == 0)
            {
                PaymentReturnDto paymentReturn = new PaymentReturnDto();
                paymentReturn.Message = "You owe no one";
                paymentReturns.Add(paymentReturn);
                return paymentReturns;
            }

            var expenseIds = payments.Select(p => p.ExpenseId).Distinct().ToList();
            var expenses = await _expenseRepository.GetListAsync(e => expenseIds.Contains(e.Id));

            var groupIds = expenses.Select(e => e.groupId).Distinct().ToList();
            var groups = await _groupRepository.GetListAsync(g => groupIds.Contains(g.Id));

            var expenseDict = expenses.ToDictionary(e => e.Id);
            var groupDict = groups.ToDictionary(g => g.Id);

            foreach (var payment in payments)
            {
                PaymentReturnDto paymentReturn = new PaymentReturnDto();
                paymentReturn.Amount = payment.Amount;
                paymentReturn.GroupName = groupDict[expenseDict[payment.ExpenseId].groupId].Name;
                paymentReturn.WhomeToGive = expenseDict[payment.ExpenseId].paidBy;
                paymentReturn.Message = "You owes from";
                paymentReturns.Add(paymentReturn);
            }

            return paymentReturns;
        }
        public async Task<List<PaymentYouGetDto>> GetWhoWillGiveToCurrentUserAsync()
        {
            List<PaymentYouGetDto> paymentYouGetReturns = new List<PaymentYouGetDto>();
            //var currentUserId = _currentUser.Id;
            var currentUserId = new Guid("3a0bc559-4a98-1dfa-317d-c94539d43a69");

            var expenses = await _expenseRepository.GetListAsync(p => p.paidBy == currentUserId);
            var expenseIds = expenses.Select(p => p.Id).Distinct().ToList();
            var groupId = expenses.Select(p => p.groupId).Distinct().ToList();
            var groups = await _groupRepository.GetListAsync(g => groupId.Contains(g.Id));

            var expenseDict = expenses.ToDictionary(e => e.Id);
            var groupDict = groups.ToDictionary(g => g.Id);


            if (expenses.Count == 0)
            {
                PaymentYouGetDto paymentReturn = new PaymentYouGetDto();
                paymentReturn.Message = "No one owes you";
                paymentYouGetReturns.Add(paymentReturn);
                return paymentYouGetReturns;
            }
            var payments = await _paymentRepository.GetListAsync(p => expenseIds.Contains(p.ExpenseId) && p.OwnedBy != currentUserId && p.IsSettled == false);

            if (payments.Count == 0)
            {
                PaymentYouGetDto paymentYouGetReturn = new PaymentYouGetDto();
                paymentYouGetReturn.Message = "All payment clear";
                paymentYouGetReturns.Add(paymentYouGetReturn);
                return paymentYouGetReturns;
            }

            foreach (var payment in payments)
            {
                PaymentYouGetDto paymentYouGetReturn = new PaymentYouGetDto();
                paymentYouGetReturn.OwesFromYou = payment.OwnedBy;
                paymentYouGetReturn.Amount = payment.Amount;
                paymentYouGetReturn.GroupName = groupDict[expenseDict[payment.ExpenseId].groupId].Name;
                paymentYouGetReturn.Message = "Owes from you";
                paymentYouGetReturns.Add(paymentYouGetReturn);
            }

            return paymentYouGetReturns;
        }




    }
}