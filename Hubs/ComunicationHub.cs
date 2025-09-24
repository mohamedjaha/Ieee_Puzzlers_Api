using IEEE_Application.DATA.Models;
using IEEE_Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace IEEE_Application.Hubs
{
    public class ComunicationHub : Hub<IComunicationHub>
    {
        private readonly Manager _manager;
        private readonly UserService _userService;
        private static readonly object _winnerLock = new object();
        private static bool _winnerDeclared = false;

        public ComunicationHub(Manager manager , UserService userService)
        {
            _manager = manager;
            _userService = userService;
        }
        public override Task OnConnectedAsync()
        {
            _manager.AddUser(Context.UserIdentifier, Context.ConnectionId);
            Console.WriteLine("A user connected: " + Context.ConnectionId);
            return base.OnConnectedAsync();
        }
        public async Task Join(string groupName , string password)
        {
            var tournament = await _userService.GetTournamentByName(groupName);
            if (tournament == null)
            {
                await Clients.Caller.PlayerLeaved("Tournament not found.");
                return;
            }
            if (tournament.Password != password)
            {
                await Clients.Caller.PlayerLeaved("Incorrect password.");
                return;
            }
            var userID = _manager.GetUserIdByConnectionId(Context.ConnectionId);
            await _userService.Add_Performance(tournament.Id , userID);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).PlayerJoined("A new player has joined the game.");

        }
        public async Task Leave(string groupName)
        {
            var userID = _manager.GetUserIdByConnectionId(Context.ConnectionId);
            if (await _userService.CheckExistingPerformance(userID, groupName))
            {
                await _userService.DeletePerformance(userID, groupName);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
                await Clients.Group(groupName).PlayerLeaved("A player has left the game.");
            }
        }
        public async Task SubmitPuzzle(string groupName, int puzzleId, string answer)
        {
            var userID = _manager.GetUserIdByConnectionId(Context.ConnectionId);

            if (await _userService.CheckExistingPerformance(userID, groupName))
            {
                if (await _userService.verifierSolution(puzzleId, answer))
                {
                    if (await _userService.UpdateSolvedCount(userID, groupName))
                    {
                        lock (_winnerLock)   
                        {
                            if (!_winnerDeclared)
                            {
                                _winnerDeclared = true;
                                AnnounceWinner(groupName);
                            }
                        }
                    }
                    else
                    {
                        await Clients.Caller.PuzzelIsCorrect("Correct solution! Puzzle solved.");
                    }
                }
                else
                {
                    await Clients.Caller.PuzzelIsWrong("Incorrect solution. Please try again.");
                }
            }
            else
            {
                await Clients.Caller.Erorr("You are not part of this tournament.");
            }
        }
        public async Task AnnounceWinner(string groupName)
        {
            await Clients.Caller.YouWin("Congratulations! You have won the game.");
            await Clients.GroupExcept(groupName, Context.ConnectionId).YouLose("Game over! You have lost the game.");    
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("A user disconnected: " + Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }

    //what server can tell clients 
    public interface IComunicationHub
    { 
        Task PlayerJoined(string Message);
        Task PlayerLeaved(string Message);
        Task YouWin(string Message);
        Task YouLose(string Message);
        Task Erorr(string Message);
        Task PuzzelIsCorrect(string Message);
        Task PuzzelIsWrong(string Message);

    }
}
