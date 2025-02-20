handlers.IncrementPlayerStat = function (args, context) {
	var playerId = currentPlayerId;
	var statName = args.statName || "Kills";
	var incrementBy = args.incrementBy || 1;

	// Lấy giá trị hiện tại của statistic
	var playerStats = server.GetPlayerStatistics({ PlayFabId: playerId });
	var currentStat = 0;

	for (var i = 0; i < playerStats.Statistics.length; i++) {
		if (playerStats.Statistics[i].StatisticName === statName) {
			currentStat = playerStats.Statistics[i].Value;
			break;
		}
	}

	// Accumulate
	var newStatValue = currentStat + incrementBy;

	// Update stats
	var updateRequest = {
		PlayFabId: playerId,
		Statistics: [
			{
				StatisticName: statName,
				Value: newStatValue,
			},
		],
	};

	server.UpdatePlayerStatistics(updateRequest);

	log.info("Updated stat:", {
		message: "Stat updated successfully!",
		statName: statName,
		newTotalValue: newStatValue,
	});

	return {
		message: "Stat updated successfully!",
		statName: statName,
		newTotalValue: newStatValue,
	};
};

handlers.GetTopScores = function (args, context) {
	var statName = args.statName || "Kills";
	var startPosition = args.startPosition || 0;
	var maxResultsCount = args.maxResultsCount || 10;

	var leaderboard = server.GetLeaderboard({
		StatisticName: statName,
		StartPosition: startPosition,
		MaxResultsCount: maxResultsCount,
	});

	var results = leaderboard.Leaderboard.map((entry) => ({
		PlayFabId: entry.PlayFabId,
		DisplayName: entry.DisplayName || "Unknown",
		Rank: entry.Position + 1,
		Score: entry.StatValue,
	}));

	return {
		leaderboard: results,
	};
};

handlers.GrantGold = function (args, context) {
	var playerId = currentPlayerId;
	var amount = args.amount || 100;

	var updateRequest = {
		PlayFabId: playerId,
		VirtualCurrency: "GC",
		Amount: amount,
	};

	var result = server.AddUserVirtualCurrency(updateRequest);
	return { message: "Gold granted successfully", newBalance: result.Balance };
};

handlers.PurchaseItem = function (args, context) {
	var playerId = currentPlayerId;
	var itemId = args.itemId;

	// Fetch Item Info from Catalog
	var catalogItem = server.GetCatalogItems({ CatalogVersion: "Main" }).Catalog.find((item) => item.ItemId === itemId);

	if (!catalogItem) {
		return { message: "Item not found in catalog." };
	}

	// Get Item Price from Catalog
	var price = catalogItem.VirtualCurrencyPrices["GC"] || 0;

	// Get Player Balance
	var playerData = server.GetUserInventory({ PlayFabId: playerId });
	var currentGold = playerData.VirtualCurrency["GC"];

	// Check if player has enough Gold
	if (currentGold < price) {
		return { message: "Not enough Gold to purchase the item." };
	}

	// Deduct Gold
	server.SubtractUserVirtualCurrency({
		PlayFabId: playerId,
		VirtualCurrency: "GC",
		Amount: price,
	});

	// Grant Item to Player
	var grantItemResult = server.GrantItemsToUser({
		PlayFabId: playerId,
		CatalogVersion: "Main",
		ItemIds: [itemId],
	});

	return { message: "Item purchased successfully!", newBalance: currentGold - price, itemId: itemId };
};
