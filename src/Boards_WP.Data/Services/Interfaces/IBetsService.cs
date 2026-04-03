using System;
using System.Collections.Generic;

using Boards_WP.Data.Models;

namespace Boards_WP.Data.Services
{
    public interface IBetsService
    {
        public Boolean IsSecretKey(String Input);
        public String ExtractBetKeywords(String Input);
        public int RegisterSecretAreaVisitAndGetTokens(int UserID);
        public int GetUserTokenCount(int UserID);
        public List<Bet> GetAllBets();
        public List<Bet> GetBetsOfUser(int UserID);
        public List<UsersBets> GetPlacedBetsOfUser(int UserID);
        public List<UsersBets> GetOngoingPlacedBetsOfUser(int UserID);
        public List<UsersBets> GetExpiredPlacedBetsOfUser(int UserID);
        public List<Bet> SearchBetsByKeywords(String Keywords);
        public Bet GetBetByID(int BetID);
        public Boolean ValidateCreateBet(int UserID, Bet CreatedBet);
        public void CreateBet(Bet CreatedBet, int CreatorID);
        public Boolean ValidatePlaceUserBet(int UserID, int Amount);
        public void PlaceUserBet(int UserID, int BetID, int Amount, BetVote Vote);
        public (decimal YesOdd, decimal NoOdd) CalculateBetOdds(int BetID, int UserID);
        public decimal GetUserTokenFeeDiscount(int UserID);
        public List<Bet> GetExpiredBetsOfUser(int UserID);
        public BetVote CheckBetCondition(int BetID);
        public void ExecuteActionsByBetResult(int UserID, int BetID);
        public Boolean didUserWinBet(int UserID, int BetID);
    }
}