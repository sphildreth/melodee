export function get()
{
    return document.cookie;
}

export function remove(key)
{
    document.cookie = key+'=; Max-Age=-99999999;';
}

export function set(key, value)
{
    document.cookie = `${key}=${value}; SameSite=Strict;path=/;`;
}
