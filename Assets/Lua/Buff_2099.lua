object:subClass("buff2099")

function buff2099.BuffOnOccur(buff)
    print("[Buff_2099] OnOccur id=" .. buff.Id ..
        " stack=" .. buff.CurrentStack ..
        " durationTimer=" .. buff.DurationTimer)
end

function buff2099.BuffRemoveOccur(buff)
    print("[Buff_2099] Remove id=" .. buff.Id ..
        " stack=" .. buff.CurrentStack)
end

function buff2099.BuffOnTick(buff, time)
    print("[Buff_2099] Tick delta=" .. time ..
        " remain=" .. buff.DurationTimer ..
        " triggerTimer=" .. buff.TriggerTimer)
    return false
end

function buff2099.BuffOnHit(buff, damageInfo, targetId)
    local value = damageInfo.Value
    local stack = buff.CurrentStack

    if stack == 1 then
        value.physics = math.floor(value.physics * 1.1)
    elseif stack == 2 then
        value.physics = math.floor(value.physics * 1.25)
    elseif stack >= 3 then
        value.physics = math.floor(value.physics * 1.5)
        value.fire = value.fire + 20
    end

    damageInfo.Value = value
    damageInfo.TriggerFlags = damageInfo.TriggerFlags + 16
    print("[Buff_2099] OnHit stack=" .. stack ..
        " target=" .. targetId ..
        " physics=" .. value.physics ..
        " fire=" .. value.fire ..
        " flags=" .. damageInfo.TriggerFlags)
    return damageInfo
end

function buff2099.BuffOnBeHurt(buff, damageInfo, attackerId)
    local value = damageInfo.Value
    local stack = buff.CurrentStack

    if stack == 1 then
        value.physics = math.floor(value.physics * 0.9)
    elseif stack == 2 then
        value.physics = math.floor(value.physics * 0.75)
    elseif stack >= 3 then
        value.physics = math.floor(value.physics * 0.5)
        value.ice = value.ice + 10
    end

    damageInfo.Value = value
    damageInfo.TriggerFlags = damageInfo.TriggerFlags + 32
    print("[Buff_2099] OnBeHurt stack=" .. stack ..
        " attacker=" .. attackerId ..
        " physics=" .. value.physics ..
        " ice=" .. value.ice ..
        " flags=" .. damageInfo.TriggerFlags)
    return damageInfo
end

function buff2099.BuffOnKill(buff, damageInfo, targetId)
    print("[Buff_2099] OnKill target=" .. targetId ..
        " stack=" .. buff.CurrentStack ..
        " flags=" .. damageInfo.TriggerFlags)
end

function buff2099.BuffOnDie(buff, damageInfo)
    print("[Buff_2099] OnDie stack=" .. buff.CurrentStack ..
        " flags=" .. damageInfo.TriggerFlags)
end

function buff2099.BuffCanRemove(buff, time, force)
    if force then
        print("[Buff_2099] Force remove")
        return true
    end

    return false
end

return buff2099
