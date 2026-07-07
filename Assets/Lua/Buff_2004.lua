object:subClass("buff2004")
local battleManager = CS.Tools.Battle.BattleManager.Instance
-- 移速提升30%，持续3秒
-- 策略是刷新持续时间
buff2004.lifeTime = 3
buff2004.lifeTimeCounter = 0

function buff2004.BuffOnOccur(buff)
	buff2004.lifeTimeCounter = 0
	property.CurrentSpeed = property.Info.Speed * (1 + 30 / 100.0)
end

function buff2004.BuffRemoveOccur(buff)
	print("buff2004 Remove")
end

function buff2004.BuffOnTick(buff, time)
	print("buff2004 OnTick")
	return false
end

function buff2004.BuffOnHit(buff, damageInfo, targetId)
	print("buff2004 OnHit")
end

function buff2004.BuffOnBeHurt(buff, damageInfo, attackerId)
	print("buff2004 OnBeHurt")
end

function buff2004.BuffOnKill(buff, damageInfo, targetId)
	print("buff2004 OnKill")
end

function buff2004.BuffCanRemove(buff, time, force)
	print("buff2004 OnCanRemove")
	return false
end

return buff2004