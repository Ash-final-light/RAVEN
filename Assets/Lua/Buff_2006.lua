object:subClass("buff2006")
local battleManager = CS.Tools.Battle.BattleManager.Instance
--（进行攻击）连击数达到5时，下一次伤害翻倍，永久buff

buff2006.combo = 0
buff2006.comboTerm = 5

buff2006.threshold = 3
buff2006.thresholdTimer = 0

buff2006.damageMultiplier = 1

function buff2006.BuffRemoveOccur(buff)
	print("buff2006 Remove")
end

function buff2006.BuffOnTick(buff, time)
	print("buff2006 OnTick")
	buff2006.thresholdTimer = buff2006.thresholdTimer + time
	if buff2006.thresholdTimer >= buff2006.threshold then
		buff2006.thresholdTimer = 0
		buff2006.combo = 0
	end
	return false
end

function buff2006.BuffOnHit(buff, damageInfo, targetId)
	print("buff2006 OnHit")
	buff2006.combo = buff2006.combo + 1
	
	damageInfo.Damage = damageInfo.Damage * buff2006.damageMultiplier
	buff2006.damageMultiplier = 1
	
	if buff2006.combo >= buff2006.comboTerm then
		buff2006.combo = 0
		buff2006.damageMultiplier = 2
	end
end

function buff2006.BuffOnBeHurt(buff, damageInfo, attackerId)
	print("buff2006 OnBeHurt")
end

function buff2006.BuffOnKill(buff, damageInfo, targetId)
	print("buff2006 OnKill")
end

function buff2006.BuffCanRemove(buff, time, force)
	print("buff2006 OnCanRemove")
	return false
end

return buff2006
