'use client'

import { Button, Dropdown } from 'flowbite-react'
import { DropdownDivider } from 'flowbite-react/lib/esm/components/Dropdown/DropdownDivider'
import { User } from 'next-auth'
import Link from "next/link"
import React from 'react'
import { AiFillCar, AiFillTrophy, AiOutlineLogout } from 'react-icons/ai'
import { HiCog, HiUser } from "react-icons/hi2"
import { signOut } from 'next-auth/react'

type Props = {
    user: Partial<User>
}

const UserActions = ({user}: Props) => {
    return (
        <Dropdown label={`Welcome ${user.name}`} inline>
            <Dropdown.Item icon={HiUser}>
                <Link href="/">
                    My Auctions
                </Link>
            </Dropdown.Item>
            <Dropdown.Item icon={AiFillTrophy}>
                <Link href="/">
                    Auctions Won
                </Link>
            </Dropdown.Item>
            <Dropdown.Item icon={AiFillCar}>
                <Link href="/">
                    Sell my car
                </Link>
            </Dropdown.Item>
            <Dropdown.Item icon={HiCog}>
                <Link href="/session">
                    Session (dev only)
                </Link>
            </Dropdown.Item>
            <DropdownDivider />
            <Dropdown.Item icon={AiOutlineLogout} onClick={() => signOut({callbackUrl: "/"})}>
                Sign out
            </Dropdown.Item>
        </Dropdown>
    )
}

export default UserActions
