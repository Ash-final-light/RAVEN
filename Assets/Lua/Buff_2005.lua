object:subClass("buff2005")
local battleManager = CS.Tools.Battle.BattleManager.Instance
-- (受到伤害)有90%几率自动释放一个基础技能（此处用print来输出），永久buff

buff2005.rate = 0.9

function buff2005.TryReleaseSkill(buff)
	if math.random() <= buff2005.rate then
		print("buff2005 Release Skill")
	end
	return false
end

function buff2005.BuffRemoveOccur(buff)
	print("buff2005 Remove")
end

function buff2005.BuffOnTick(buff, time)
	print("buff2005 OnTick")
	return false
end

function buff2005.BuffOnHit(buff, damageInfo, targetId)
	print("buff2005 OnHit")
end

function buff2005.BuffOnBeHurt(buff, damageInfo, attackerId)
	print("buff2005 OnBeHurt")
	buff2005.TryReleaseSkill(buff)
end

function buff2005.BuffOnKill(buff, damageInfo, targetId)
	print("buff2005 OnKill")
end

function buff2005.BuffCanRemove(buff, time, force)
	print("buff2005 OnCanRemove")
	return false
end

return buff2005